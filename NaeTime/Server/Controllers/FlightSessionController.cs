using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Models;
using NaeTime.Core.Models;
using NaeTime.Core.Processors;
using NaeTime.Node.Client.Abstractions;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Server.Controllers
{
    public class FlightSessionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly INaeTimeUnitOfWork _unitOfWork;
        private readonly INodeClientFactory _nodeClientFactory;

        public FlightSessionController(UserManager<ApplicationUser> userManager, IMapper mapper, INaeTimeUnitOfWork unitOfWork, INodeClientFactory nodeClientFactory)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _nodeClientFactory = nodeClientFactory ?? throw new ArgumentNullException(nameof(nodeClientFactory));
        }

        [Authorize]
        [HttpPost("session/start")]
        public async Task<IActionResult> CreateSession([FromBody]FlyingSessionDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var session = new FlyingSession()
            {
                HostPilotId = user.PilotId,
                Start = DateTime.UtcNow,
                Name = dto.Name,
                IsPublic = dto.IsPublic,
                TrackId = dto.TrackId,
                AllowedPilots = new List<Pilot>(),
            };

            _unitOfWork.FlyingSessions.Insert(session);

            await _unitOfWork.SaveChangesAsync();

            var sessionDto = _mapper.Map<FlyingSession, FlyingSessionDto>(session);

            return Created($"session/{session.Id}", sessionDto);
        }

        [Authorize]
        [HttpPost("session/{id:Guid}/stop/")]
        public async Task<ActionResult<FlyingSessionDto>> StopSession(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var session = await _unitOfWork.FlyingSessions.GetAsync(id);

            if (session == null)
            {
                return NoContent();
            }

            if (session.HostPilotId != user.PilotId)
            {
                return Forbid();
            }

            if (session.End != null)
            {
                return BadRequest("Session_already_ended");
            }
            var endDate = DateTime.UtcNow;
            session.End = endDate;

            foreach (var flight in session.Flights)
            {
                if (flight.End == null)
                {
                    flight.End = endDate;
                    await StopFlightStreams(flight);
                }
            }

            return Ok();
        }

        private async Task StopFlightStreams(Flight flight)
        {
            foreach (var stream in flight.RssiStreams)
            {
                var streamNode = await _unitOfWork.Nodes.GetAsync(stream.NodeId);
                if (streamNode != null)
                {
                    var nodeClient = _nodeClientFactory.CreateClient(streamNode.Address);
                    await nodeClient.StopRssiStreamAsync(stream.Id);
                }
            }
        }

        [Authorize]
        [HttpGet("session/{id:Guid}")]
        public async Task<ActionResult<FlyingSessionDto>> GetSession(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var session = await _unitOfWork.FlyingSessions.GetAsync(id);
            if (session == null)
            {
                return NoContent();
            }

            if (session.HostPilotId != user.PilotId && !session.AllowedPilots.Any(x => x.Id == user.PilotId))
            {
                return Forbid();
            }

            return _mapper.Map<FlyingSession, FlyingSessionDto>(session);
        }

        [Authorize]
        [HttpGet("session/my")]
        public async Task<ActionResult<List<FlyingSessionDto>>> GetForPilot()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var sessions = await _unitOfWork.FlyingSessions.GetAllowedAsync(user.PilotId);

            return _mapper.Map<List<FlyingSession>, List<FlyingSessionDto>>(sessions);
        }

        [Authorize]
        [HttpPost("session/{sessionId:Guid}/flight/start")]
        public async Task<ActionResult<FlightDto>> CreateFlight(Guid sessionId, [FromQuery] int frequency)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var session = await _unitOfWork.FlyingSessions.GetAsync(sessionId);
            if (session == null)
            {
                return NoContent();
            }
            if (session.HostPilotId != user.PilotId && !session.AllowedPilots.Any(x => x.Id == user.PilotId))
            {
                return Forbid();
            }
            var track = await _unitOfWork.Tracks.GetAsync(session.TrackId);

            if (track == null)
            {
                return NoContent();
            }

            var flight = new Flight()
            {
                Start = DateTime.UtcNow,
                PilotId = user.PilotId,
                Frequency = frequency,
                TrackId = session.TrackId
            };
            foreach(var gate in track.Gates)
            {
                var rssiStream = new RssiStream()
                {
                    NodeId = gate.NodeId,
                };
                flight.RssiStreams.Add(rssiStream);
            }

            session.Flights.Add(flight);

            _unitOfWork.FlyingSessions.Update(session);
            await _unitOfWork.SaveChangesAsync();

            foreach(var stream in flight.RssiStreams)
            {
                var node = await _unitOfWork.Nodes.GetAsync(stream.NodeId);
                if(node != null)
                {
                    var nodeClient = _nodeClientFactory.CreateClient(node.Address);
                    var streamDto = await nodeClient.StartRssiStreamAsync(stream.Id, flight.Frequency, null);

                    _mapper.Map(streamDto, stream);
                }
            }

            await _unitOfWork.SaveChangesAsync();

            var flightDto = _mapper.Map<Flight, FlightDto>(flight);

            return Created($"session/{sessionId}/flight/{flight.Id}", flightDto);
        }
        [Authorize]
        [HttpPost("session/flight/{flightId:Guid}/stop")]
        public async Task<IActionResult> StopFlight(Guid flightId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var session = await _unitOfWork.FlyingSessions.GetForFlightAsync(flightId);
            if (session == null)
            {
                return NoContent();
            }

            var flight = session.Flights.FirstOrDefault(x => x.Id == flightId);
            if (flight == null)
            {
                return NoContent();
            }
            if (flight.PilotId != user.PilotId)
            {
                return Forbid();
            }
            flight.End = DateTime.UtcNow;
            await StopFlightStreams(flight);
            _unitOfWork.FlyingSessions.Update(session);

            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("session/flight/{flightId:Guid}/recalculate")]
        public async Task<IActionResult> Recalculate(Guid flightId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var flight = await _unitOfWork.Flights.GetWithReadings(flightId);
            if (flight == null)
            {
                return NoContent();
            }

            if (flight.PilotId != user.PilotId)
            {
                return Forbid();
            }

            return Ok();
        }
    }
}
