using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects.Track;
using System.Collections.Concurrent;

namespace NaeTime.Client.WebApi.Controllers;

[Route("[controller]")]
public class TrackController : Controller
{
    public static ConcurrentDictionary<Guid, TrackDetails> TrackDetails = new();

    [HttpPut("update")]
    public IActionResult UpdateTrack([FromBody] UpdateTrack dto)
    {
        if (!TrackDetails.TryGetValue(dto.Id, out var details))
        {
            return NotFound();
        }

        var updatedDetails = new TrackDetails(dto.Id, dto.Name, dto.TimedGates);
        TrackDetails.AddOrUpdate(dto.Id, updatedDetails, (id, old) => updatedDetails);

        return Ok(updatedDetails);
    }
    [HttpPost("create")]
    public IActionResult CreateTrack([FromBody] CreateTrack dto)
    {
        if (string.IsNullOrEmpty(dto.Name))
        {
            return BadRequest("Name is required");
        }


        var details = new TrackDetails(Guid.NewGuid(), dto.Name, dto.TimedGates);
        TrackDetails.TryAdd(details.Id, details);
        return Created($"track/{details.Id}", details);
    }
    [HttpGet("{trackId:Guid}")]
    public ActionResult<TrackDetails> GetTrack(Guid trackId)
    {
        if (!TrackDetails.TryGetValue(trackId, out var details))
        {
            return NotFound();
        }

        return Ok(details);
    }

    [HttpGet("all")]
    public ActionResult<IEnumerable<TrackDetails>> GetAll()
    {
        return Ok(TrackDetails.Values);
    }
}
