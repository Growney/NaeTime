using Microsoft.AspNetCore.Mvc;
using NaeTime.Client.Shared.DataTransferObjects;
using System.Collections.Concurrent;

namespace NaeTime.Client.WEBAPI.Controllers;

[Route("[controller]")]
public class HardwareController : Controller
{
    private static ConcurrentDictionary<Guid, EthernetLapRF8ChannelTimerDetails> _ethernetLapRF8ChannelTimers = new();

    static HardwareController()
    {
        var testTimer = new EthernetLapRF8ChannelTimerDetails(Guid.NewGuid(), "LapRF 8 Channel Timer", "192.168.28.5", 5403);
        _ethernetLapRF8ChannelTimers.TryAdd(testTimer.Id, testTimer);
    }

    [HttpGet("timer/details/all")]
    public ActionResult<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        var results = new List<TimerDetails>();

        results.AddRange(_ethernetLapRF8ChannelTimers.Values.Select(x => new TimerDetails(x.Id, x.Name, TimerType.EthernetLapRF8Channel)));

        return Ok(results);
    }

    [HttpPost("ethernetlaprf8channel/create")]
    public IActionResult CreateEthernetLapRF8ChannelTimer([FromBody] CreateEthernetLapRF8ChannelTimer dto)
    {
        var details = new EthernetLapRF8ChannelTimerDetails(Guid.NewGuid(), dto.Name, dto.IpAddress, dto.Port);
        _ethernetLapRF8ChannelTimers.TryAdd(details.Id, details);
        return Created($"hardware/ethernetlaprf8channel/{details.Id}", details);
    }
    [HttpGet("ethernetlaprf8channel/{timerId:guid}")]
    public ActionResult<EthernetLapRF8ChannelTimerDetails> GetEthernetLapRF8ChannelTimer(Guid timerId)
    {
        if (!_ethernetLapRF8ChannelTimers.TryGetValue(timerId, out var details))
        {
            return NotFound();
        }

        return Ok(details);
    }
    [HttpPut("ethernetlaprf8channel/update")]
    public IActionResult UpdateEthernetLapRF8ChannelTimer([FromBody] EthernetLapRF8ChannelTimerDetails dto)
    {
        if (!_ethernetLapRF8ChannelTimers.TryGetValue(dto.Id, out var details))
        {
            return NotFound();
        }

        _ethernetLapRF8ChannelTimers.AddOrUpdate(dto.Id, dto, (id, old) => dto);

        return Ok(details);
    }
    [HttpGet("ethernetlaprf8channel/all")]
    public ActionResult<IEnumerable<EthernetLapRF8ChannelTimerDetails>> GetAllLapRF8ChannelTimers()
    {
        return Ok(_ethernetLapRF8ChannelTimers.Values);
    }

}
