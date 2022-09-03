using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Models;
using NaeTime.Core.Models;
using NaeTime.Shared.Client;

namespace NaeTime.Server.Controllers
{
    public class TrackController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly INaeTimeUnitOfWork _unitOfWork;

        public TrackController(UserManager<ApplicationUser> userManager, IMapper mapper, INaeTimeUnitOfWork unitOfWork)
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        }
        [Authorize]
        [HttpGet("track/all")]
        public async Task<ActionResult<List<TrackDto>>> GetTracks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var userTracks = await _unitOfWork.Tracks.GetCreatedByPilotAsync(user.PilotId);

            return _mapper.Map<List<Track>, List<TrackDto>>(userTracks);
        }
        [Authorize]
        [HttpGet("track/{id:Guid?}")]
        public async Task<ActionResult<TrackDto>> Track(Guid? id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            if(id == null)
            {
                return new TrackDto();
            }

            var track = await _unitOfWork.Tracks.GetAsync(id.Value);

            if (track == null)
            {
                return NoContent();
            }
            if (track.CreatorPilotId != user.PilotId)
            {
                return Forbid();
            }
            return _mapper.Map<Track, TrackDto>(track);
        }
        [Authorize]
        [HttpPost("track")]
        public async Task<IActionResult> Track([FromBody] TrackDto trackDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var track = await _unitOfWork.Tracks.GetAsync(trackDto.Id);
            bool newTrack = false;

            if (track != null && track.CreatorPilotId != user.PilotId)
            {
                return Forbid();
            }

            if (track != null)
            {
                _mapper.Map(trackDto, track);
            }
            else
            {
                newTrack = true;
                track = _mapper.Map<Abstractions.Models.Track>(trackDto);
            }

            track.CreatorPilotId = user.PilotId;

            //TODO validate track
            if (newTrack)
            {
                _unitOfWork.Tracks.Insert(track);
            }
            else
            {
                _unitOfWork.Tracks.Update(track);
            }

            await _unitOfWork.SaveChangesAsync();

            return Ok();

        }
    }
}
