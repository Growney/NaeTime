using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Core.Models;
using NaeTime.Shared.Client;

namespace NaeTime.Server.Controllers
{
    [Authorize]
    public class TrackController : Controller
    {
        public TrackController()
        {
        }
        [HttpGet("track/all")]
        public async Task<ActionResult<List<TrackDto>>> GetTracks()
        {
            throw new NotImplementedException();
        }
        [HttpGet("track/{id:Guid?}")]
        public async Task<ActionResult<TrackDto>> Track(Guid? id)
        {

            throw new NotImplementedException();
        }
        [HttpPost("track")]
        public async Task<IActionResult> Track([FromBody] TrackDto trackDto)
        {
            throw new NotImplementedException();

        }
    }
}
