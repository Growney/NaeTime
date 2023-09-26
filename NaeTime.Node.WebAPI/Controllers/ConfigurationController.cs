using Mapping.Abstractions;
using Microsoft.AspNetCore.Mvc;
using NaeTime.Node.Abstractions.Domain;
using NaeTime.Node.Abstractions.Models;
using NaeTime.Node.Abstractions.Repositories;
using NaeTime.Node.WebAPI.Shared.Models;
using Tensor.Mapping.Abstractions;

namespace NaeTime.Node.WebAPI.Controllers;

[ApiController]
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    private readonly IMapper _mapper;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INodeManager _nodeManager;

    public ConfigurationController(ILogger<ConfigurationController> logger, IUnitOfWork unitOfWork, IMapper mapper, INodeManager nodeManager)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _nodeManager = nodeManager;
    }

    [HttpGet("/configuration")]
    public async Task<ActionResult<NodeConfigurationDto>> Get()
    {
        var configuration = await _unitOfWork.ConfigurationRepository.GetNodeConfiguration();

        var dto = _mapper.Map<NodeConfigurationDto>(configuration);

        if (configuration != null)
        {
            return Ok(dto);
        }
        else
        {
            return NoContent();
        }
    }
    [HttpPost("/configuration")]
    public async Task<IActionResult> Set(NodeConfigurationDto dto)
    {
        var domain = _mapper.Map<NodeConfiguration>(dto);

        _unitOfWork.ConfigurationRepository.SetConfiguration(domain);

        await _unitOfWork.CommitAsync();

        await _nodeManager.SetConfiguration(domain);

        return Ok();
    }
}