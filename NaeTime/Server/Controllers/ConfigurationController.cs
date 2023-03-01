using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Abstractions;
using NaeTime.Abstractions.Models;
using NaeTime.Core.Models;
using NaeTime.Shared.Client;
using NaeTime.Shared.Node;

namespace NaeTime.Server.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly HttpClient _httpClient;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IValidator<Abstractions.Models.Node> _nodeValidator;
        private readonly INaeTimeUnitOfWork _unitOfWork;

        public ConfigurationController(INaeTimeUnitOfWork unitOfWork, IValidator<Abstractions.Models.Node> nodeValidator, IMapper mapper, UserManager<ApplicationUser> userManager, HttpClient httpClient)
        {
            _unitOfWork = unitOfWork;
            _nodeValidator = nodeValidator;
            _mapper = mapper;
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _userManager = userManager;
        }
        [Authorize]
        [HttpGet("configuration")]
        public async Task<ActionResult<SystemConfigurationDto>> GetUserConfiguration()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var nodes = await _unitOfWork.Nodes.GetForPilotAsync(user.PilotId);

            var nodeDtos = _mapper.Map<List<Abstractions.Models.Node>, List<NodeDto>>(nodes);

            return new SystemConfigurationDto()
            {
                Nodes = nodeDtos,
            };
        }
        [Authorize]
        [HttpGet("configuration/node")]
        public async Task<ActionResult<List<NodeDto>>> GetNodes()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            var nodes = await _unitOfWork.Nodes.GetForPilotAsync(user.PilotId);

            return _mapper.Map<List<Abstractions.Models.Node>, List<NodeDto>>(nodes);
        }

        [Authorize]
        [HttpPost("configuration/node")]
        public async Task<IActionResult> Node([FromBody] NodeDto? nodeDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            if (nodeDto == null)
            {
                return BadRequest("Node configuration not provided");
            }

            Abstractions.Models.Node? node = null;
            bool newNode = false;
            if (nodeDto.Id != null)
            {
                node = await _unitOfWork.Nodes.GetAsync(nodeDto.Id.Value);
            }

            if (node != null && node.PilotId != user.PilotId)
            {
                return Forbid();
            }

            if (node != null)
            {
                _mapper.Map<NodeDto, Abstractions.Models.Node>(nodeDto, node);
            }
            else
            {
                newNode = true;
                node = _mapper.Map<NodeDto, Abstractions.Models.Node>(nodeDto);
            }
            node.PilotId = user.PilotId;

            var validationResult = _nodeValidator.Validate(node);
            if (!validationResult.IsValid)
            {
                return BadRequest("Node invalid");
            }


            if (newNode)
            {
                _unitOfWork.Nodes.Insert(node);
            }
            else
            {
                _unitOfWork.Nodes.Update(node);
            }

            await _unitOfWork.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpPost("configuration/node/setup/{id}")]
        public async Task<IActionResult> SetupNode(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var node = await _unitOfWork.Nodes.GetAsync(id);
            if (node == null)
            {
                return NotFound();
            }
            if (node.PilotId != user.PilotId)
            {
                return Unauthorized();
            }

            _httpClient.BaseAddress = new Uri(node.Address);
            var nodeConfiguration = _mapper.Map<Abstractions.Models.Node, ConfigurationDto>(node);
            nodeConfiguration.ServerUri = $"{Request.Scheme}://{Request.Host}";
            var configResponse = await _httpClient.PostAsJsonAsync("config", nodeConfiguration);
            var response = await configResponse.Content.ReadAsStringAsync();
            if (configResponse.IsSuccessStatusCode)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpGet("configuration/node/test/{id}")]
        public async Task<ActionResult<NodeTestDto>> TestNode(Guid id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var node = await _unitOfWork.Nodes.GetAsync(id);
            if (node == null)
            {
                return NotFound();
            }
            if (node.PilotId != user.PilotId)
            {
                return Unauthorized();
            }

            bool isAvailable = false;
            bool isSetup = false;
            _httpClient.BaseAddress = new Uri(node.Address);
            var configResponse = await _httpClient.GetAsync("config");
            var rx5808Tests = new List<RX5808ReceiverTestDto>();
            if (configResponse.IsSuccessStatusCode)
            {
                isAvailable = true;
                try
                {
                    var deviceConfigDto = await configResponse.Content.ReadFromJsonAsync<ConfigurationDto>();
                    if (deviceConfigDto?.Id == node.Id)
                    {
                        isSetup = true;

                        if (node.RX5808Receivers != null)
                        {
                            foreach (var rx5808 in node.RX5808Receivers)
                            {
                                var deviceRx5808Dto = deviceConfigDto?.RX5808Receivers?.FirstOrDefault(x => x.Id == rx5808.Id);
                                if (deviceRx5808Dto != null)
                                {
                                    rx5808Tests.Add(new RX5808ReceiverTestDto()
                                    {
                                        Id = deviceRx5808Dto.Id,
                                        IsConfigured = true,
                                    });
                                }

                            }
                        }

                    }
                }
                catch
                {

                }

            }

            return new NodeTestDto()
            {
                NodeId = id,
                IsAvailable = isAvailable,
                IsSetup = isSetup,
            };
        }
    }
}
