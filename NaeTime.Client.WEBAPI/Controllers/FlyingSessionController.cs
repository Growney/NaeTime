using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects.FlyingSession;
using System.Collections.Concurrent;

namespace NaeTime.Client.WebApi.Controllers;
[Route("[controller]")]
public class FlyingSessionController : Controller
{
    private static ConcurrentDictionary<Guid, FlyingSessionDetails> _flyingSessionDetails = new();

    [HttpPut("update")]
    public IActionResult UpdateFlyingSession([FromBody] UpdateFlyingSession dto)
    {
        if (!_flyingSessionDetails.TryGetValue(dto.Id, out var details))
        {
            return NotFound();
        }

        if (!TrackController.TrackDetails.TryGetValue(dto.TrackId, out var trackDetails))
        {
            return NotFound();
        }


        var updatedDetails = new FlyingSessionDetails(dto.Id, dto.Description, dto.Start, dto.ExpectedEnd, dto.TrackId);
        _flyingSessionDetails.AddOrUpdate(dto.Id, updatedDetails, (id, old) => updatedDetails);

        return Ok(updatedDetails);
    }
    [HttpPost("create")]
    public IActionResult CreateFlyingSession([FromBody] CreateFlyingSession dto)
    {
        if (string.IsNullOrEmpty(dto.Description))
        {
            return BadRequest("Name is required");
        }
        var details = new FlyingSessionDetails(Guid.NewGuid(), dto.Description, dto.Start, dto.ExpectedEnd, dto.TrackId);
        _flyingSessionDetails.TryAdd(details.Id, details);
        return Created($"flyingsession/{details.Id}", details);
    }
    [HttpGet("{flyingSessionId:Guid}")]
    public ActionResult<FlyingSessionDetails> GetFlyingSession(Guid flyingSessionId)
    {
        if (!_flyingSessionDetails.TryGetValue(flyingSessionId, out var details))
        {
            return NotFound();
        }

        var trackSynced = new FlyingSessionDetails(details.Id, details.Description, details.Start, details.ExpectedEnd, details.TrackId);

        return Ok(trackSynced);
    }
    [HttpGet("all")]
    public ActionResult<IEnumerable<FlyingSessionDetails>> GetAll()
    {
        var all = new List<FlyingSessionDetails>();
        foreach (var session in _flyingSessionDetails)
        {
            all.Add(new FlyingSessionDetails(session.Value.Id, session.Value.Description, session.Value.Start, session.Value.ExpectedEnd, session.Value.TrackId));
        }

        return Ok(all);
    }
}
