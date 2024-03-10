
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace NaeTime.Persistence;
internal class HardwareService : ISubscriber
{
    private readonly HardwareDbContext _dbcontext;

    public HardwareService(HardwareDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task When(EthernetLapRF8ChannelConfigured configuredEvent)
    {
        var existingTimer = await _dbcontext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == configuredEvent.TimerId).ConfigureAwait(false);

        if (existingTimer == null)
        {
            existingTimer = new EthernetLapRF8Channel
            {
                Id = configuredEvent.TimerId,
            };
            _dbcontext.EthernetLapRF8Channels.Add(existingTimer);
        }

        existingTimer.Name = configuredEvent.Name;
        existingTimer.IpAddress = configuredEvent.IpAddress.GetAddressBytes();
        existingTimer.Port = configuredEvent.Port;

        await _dbcontext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(TimerConnectionEstablished connectionEstablishedEvent)
    {
        var existingStatus = await _dbcontext.TimerStatuses.FirstOrDefaultAsync(x => x.Id == connectionEstablishedEvent.TimerId).ConfigureAwait(false);
        if (existingStatus == null)
        {
            existingStatus = new TimerStatus
            {
                Id = connectionEstablishedEvent.TimerId,
            };
            _dbcontext.TimerStatuses.Add(existingStatus);
        }

        existingStatus.WasConnected = true;
        existingStatus.ConnectionStatusChanged = connectionEstablishedEvent.UtcTime;

        await _dbcontext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(TimerDisconnected disconnectedEvent)
    {
        var existingStatus = await _dbcontext.TimerStatuses.FirstOrDefaultAsync(x => x.Id == disconnectedEvent.TimerId).ConfigureAwait(false);
        if (existingStatus == null)
        {
            existingStatus = new TimerStatus
            {
                Id = disconnectedEvent.TimerId,
            };
            _dbcontext.TimerStatuses.Add(existingStatus);
        }

        existingStatus.WasConnected = false;
        existingStatus.ConnectionStatusChanged = disconnectedEvent.UtcTime;

        await _dbcontext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<TimerDetailsResponse> On(TimerDetailsRequest request)
    {
        var lapRF8Channels = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new TimerDetailsResponse.TimerDetails(x.Id, x.Name, TimerDetailsResponse.TimerType.EthernetLapRF8Channel, 8))
            .ToListAsync().ConfigureAwait(false);

        return new TimerDetailsResponse(lapRF8Channels);
    }
    public async Task<EthernetLapRF8ChannelTimersResponse> On(EthernetLapRF8ChannelTimersRequest request)
    {
        var timerDetails = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new EthernetLapRF8ChannelTimersResponse.EthernetLapRF8Channel(x.Id, new IPAddress(x.IpAddress), x.Port))
            .ToListAsync().ConfigureAwait(false);

        return new EthernetLapRF8ChannelTimersResponse(timerDetails);
    }
    public async Task<EthernetLapRF8ChannelResponse?> On(EthernetLapRF8ChannelRequest request)
    {
        var timer = await _dbcontext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == request.TimerId).ConfigureAwait(false);

        return timer == null ? null : new EthernetLapRF8ChannelResponse(timer.Id, timer.Name, new IPAddress(timer.IpAddress), timer.Port);
    }
}
