using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects.Pilot;
using System.Collections.Concurrent;

namespace NaeTime.Client.WebApi.Controllers;
[Route("[controller]")]
public class PilotController : Controller
{
    private static ConcurrentDictionary<Guid, PilotDetails> _pilotDetails = new();

    static PilotController()
    {
        var testPilot1 = new PilotDetails(new Guid("20a1745f-6ea8-40a0-afc0-bdf0aa2afd67"), "Graeme", null, "Just G");
        _pilotDetails.TryAdd(testPilot1.Id, testPilot1);
    }
    [HttpPut("update")]
    public IActionResult UpdatePilot([FromBody] UpdatePilot dto)
    {
        var details = new PilotDetails(dto.Id, dto.FirstName, dto.LastName, dto.CallSign);
        _pilotDetails.AddOrUpdate(details.Id, details, (id, old) => details);
        return Ok(details);
    }
    [HttpPost("create")]
    public IActionResult CreatePilot([FromBody] CreatePilot dto)
    {
        var details = new PilotDetails(Guid.NewGuid(), dto.FirstName, dto.LastName, dto.CallSign);
        _pilotDetails.TryAdd(details.Id, details);
        return Created($"pilot/{details.Id}", details);
    }
    [HttpGet("{pilotId:Guid}")]
    public ActionResult<PilotDetails> GetPilot(Guid pilotId)
    {
        if (!_pilotDetails.TryGetValue(pilotId, out var details))
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
