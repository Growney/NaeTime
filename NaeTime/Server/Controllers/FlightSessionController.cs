using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Core.Models;
using NaeTime.Node.Client.Abstractions;
using NaeTime.Shared.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaeTime.Server.Controllers
{
    [Authorize]
    public class FlightSessionController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public FlightSessionController(INodeClientFactory nodeClientFactory)
        {
            
        }

        [HttpPost("session/start")]
        public async Task<IActionResult> CreateSession([FromBody]FlyingSessionDto dto)
        {
            throw new NotImplementedException();
        }

        [HttpPost("session/{id:Guid}/stop/")]
        public async Task<ActionResult<FlyingSessionDto>> StopSession(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("session/{id:Guid}")]
        public async Task<ActionResult<FlyingSessionDto>> GetSession(Guid id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("session/my")]
        public async Task<ActionResult<List<FlyingSessionDto>>> GetForPilot()
        {

            throw new NotImplementedException();
        }

        [HttpPost("session/{sessionId:Guid}/flight/start")]
        public async Task<ActionResult<FlightDto>> CreateFlight(Guid sessionId, [FromQuery] int frequency)
        {

            throw new NotImplementedException();
        }
        [HttpPost("session/flight/{flightId:Guid}/stop")]
        public async Task<IActionResult> StopFlight(Guid flightId)
        {
            throw new NotImplementedException();
        }

        [HttpPost("session/flight/{flightId:Guid}/recalculate")]
        public async Task<IActionResult> Recalculate(Guid flightId)
        {
            throw new NotImplementedException();
        }
    }
}
