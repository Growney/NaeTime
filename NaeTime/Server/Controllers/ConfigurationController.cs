using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Shared.Client;
using NaeTime.Shared.Node;

namespace NaeTime.Server.Controllers
{
    [Authorize]
    public class ConfigurationController : Controller
    {
        private readonly HttpClient _httpClient;
        public ConfigurationController(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }
        [HttpGet("configuration")]
        public async Task<ActionResult<SystemConfigurationDto>> GetUserConfiguration()
        {
            throw new NotImplementedException();
        }
        [HttpGet("configuration/node")]
        public async Task<ActionResult<List<NodeDto>>> GetNodes()
        {
            throw new NotImplementedException();
        }

        [HttpPost("configuration/node")]
        public async Task<IActionResult> Node([FromBody] NodeDto? nodeDto)
        {
            throw new NotImplementedException();
        }

        [HttpPost("configuration/node/setup/{id}")]
        public async Task<IActionResult> SetupNode(Guid id)
        {
            throw new NotImplementedException();
        }

        [Authorize]
        [HttpGet("configuration/node/test/{id}")]
        public async Task<ActionResult<NodeTestDto>> TestNode(Guid id)
        {
            throw new NotImplementedException();
        }
    }
}
