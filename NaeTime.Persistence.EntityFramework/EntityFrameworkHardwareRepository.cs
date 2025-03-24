using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions.Hardware;
using System.Net;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkHardwareRepository : IHardwareRepository
{
    private readonly NaeTimeDbContext _dbcontext;

    public EntityFrameworkHardwareRepository(NaeTimeDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task<IEnumerable<SerialEsp32Node>> GetAllSerialEsp32NodeTimers()
    {
        List<SerialEsp32Node> serialNodes = await _dbcontext.SerialEsp32Nodes
            .Select(x => new SerialEsp32Node(x.Id, x.Name, x.Port))
            .ToListAsync().ConfigureAwait(false);

        return serialNodes;
    }

    public async Task<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        List<TimerDetails> lapRF8Channels = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new TimerDetails(x.Id, x.Name, TimerType.EthernetLapRF8Channel, 8))
            .ToListAsync().ConfigureAwait(false);

        lapRF8Channels.AddRange(await _dbcontext.SerialEsp32Nodes
            .Select(x => new TimerDetails(x.Id, x.Name, TimerType.SerialEsp32Node, 6))
            .ToListAsync().ConfigureAwait(false));

        //when there are more timer types will need to add them all to a larger list

        return lapRF8Channels;
    }
    public async Task<EthernetLapRF8ChannelTimer?> GetEthernetLapRF8ChannelTimer(Guid timerId)
    {
        var timer = await _dbcontext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == timerId).ConfigureAwait(false);

        return timer == null ? null : new EthernetLapRF8ChannelTimer(timer.Id, timer.Name, new IPAddress(timer.IpAddress), timer.Port);
    }
    public async Task<IEnumerable<EthernetLapRF8ChannelTimer>> GetAllEthernetLapRF8ChannelTimers()
    {
        List<EthernetLapRF8ChannelTimer> timerDetails = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new EthernetLapRF8ChannelTimer(x.Id, x.Name, new IPAddress(x.IpAddress), x.Port))
            .ToListAsync().ConfigureAwait(false);

        return timerDetails;
    }
    public async Task<SerialEsp32Node?> GetSerialEsp32NodeTimer(Guid timerId)
    {
        var timer = await _dbcontext.SerialEsp32Nodes.FirstOrDefaultAsync(x => x.Id == timerId).ConfigureAwait(false);

        return timer == null ? null : new SerialEsp32Node(timer.Id, timer.Name, timer.Port);
    }
    public async Task<TimerType> GetTimerType(Guid timerId)
    {
        if (await _dbcontext.EthernetLapRF8Channels.AnyAsync(x => x.Id == timerId))
        {
            return TimerType.EthernetLapRF8Channel;
        }

        if (await _dbcontext.SerialEsp32Nodes.AnyAsync(x => x.Id == timerId))
        {
            return TimerType.SerialEsp32Node;
        }

        return TimerType.Unknown;
    }
}
