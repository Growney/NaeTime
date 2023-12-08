using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects;
using System.Collections.Concurrent;

namespace NaeTime.Client.WEBAPI.Controllers;

[Route("[controller]")]
public class HardwareController : Controller
{
    private static ConcurrentDictionary<Guid, LapRF8ChannelTimerDetails> _lapRF8ChannelTimers = new();

    [HttpGet("timer/details/all")]
    public ActionResult<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        var results = new List<TimerDetails>();

        results.AddRange(_lapRF8ChannelTimers.Values.Select(x => new TimerDetails(x.Id, x.Name, TimerType.LapRF8Channel)));

        return Ok(results);
    }

    [HttpPost("laprf8channel/create")]
    public IActionResult CreateLapRF8ChannelTimer(CreateLapRF8ChannelTimer dto)
    {
        var details = new LapRF8ChannelTimerDetails(Guid.NewGuid(), dto.Name, dto.IpAddress, dto.Port);
        _lapRF8ChannelTimers.TryAdd(details.Id, details);
        return Created($"hardware/laprf8channel/{details.Id}", details);
    }
    [HttpGet("laprf8channel/{timerId:guid}")]
    public ActionResult<LapRF8ChannelTimerDetails> GetLapRF8ChannelTimer(Guid timerId)
    {
        if (!_lapRF8ChannelTimers.TryGetValue(timerId, out var details))
        {
            return NotFound();
        }

        return Ok(details);
    }
    [HttpGet("laprf8channel/all")]
    public ActionResult<IEnumerable<LapRF8ChannelTimerDetails>> GetAllLapRF8ChannelTimers()
    {
        return Ok(_lapRF8ChannelTimers.Values);
    }

}
