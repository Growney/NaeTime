using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Models;
using NaeTime.Core.Models;
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

        public FlightSessionController(UserManager<ApplicationUser> userManager, IMapper mapper, INaeTimeUnitOfWork unitOfWork)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }

        [Authorize]
        [HttpPost("session/start")]
        public async Task<IActionResult> CreateSession(FlyingSessionDto dto)
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
                AllowedPilots = (from pilotId in dto.AllowedPilots select new AllowedPilot() { PilotId = pilotId }).ToList(),
            };

            _unitOfWork.FlyingSessions.Insert(session);

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
                }
                //TODO STOP FLIGHT STREAMS
            }

            return Ok();
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

            if (session.HostPilotId != user.PilotId && !session.AllowedPilots.Any(x => x.PilotId == user.PilotId))
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
        public async Task<IActionResult> CreateFlight(Guid sessionId, [FromQuery] int frequency)
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
            if (session.HostPilotId != user.PilotId || !session.AllowedPilots.Any(x => x.PilotId == user.PilotId))
            {
                return Forbid();
            }
            var flight = new Flight()
            {
                Start = DateTime.UtcNow,
                PilotId = user.PilotId,
                Frequency = frequency,
            };
            //TODO Start flight streams

            session.Flights.Add(flight);

            _unitOfWork.FlyingSessions.Update(session);

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

            _unitOfWork.FlyingSessions.Update(session);

            await _unitOfWork.SaveChangesAsync();
            //TODO stop flight streams
            return Ok();
        }
    }
}
