using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects;
using System.Collections.Concurrent;

namespace NaeTime.Client.WEBAPI.Controllers;
[Route("[controller]")]
public class PilotController : Controller
{
    private static ConcurrentDictionary<Guid, PilotDetails> _pilotDetails = new();
    [HttpPost("pilot/create")]
    public IActionResult CreatePilot(CreatePilot dto)
    {
        var details = new PilotDetails(Guid.NewGuid(), dto.FirstName, dto.LastName, dto.CallSign);
        _pilotDetails.TryAdd(details.Id, details);
        return Created($"pilot/{details.Id}", details);
    }
    [HttpGet("{pilotId:guid}")]
    public ActionResult<PilotDetails> GetPilot(Guid pilot)
    {
        if (!_pilotDetails.TryGetValue(pilot, out var details))
        {
            return NotFound();
        }

        return Ok(details);
    }
    [HttpGet("all")]
    public ActionResult<IEnumerable<PilotDetails>> GetAll()
    {
        return Ok(_pilotDetails.Values);
    }
}
