
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace NaeTime.Hardware.SQLite;
internal class HardwareService
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

    public async Task<IEnumerable<Messages.Models.TimerDetails>> GetAllTimerDetails()
    {
        var lapRF8Channels = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new Messages.Models.TimerDetails(x.Id, x.Name, Messages.Models.TimerType.EthernetLapRF8Channel, 8))
            .ToListAsync().ConfigureAwait(false);

        //when there are more timer types will need to add them all to a larger list

        return lapRF8Channels;
    }
    public async Task<IEnumerable<Messages.Models.EthernetLapRF8ChannelTimer>> GetAllEthernetLapRF8ChannelTimers()
    {
        var timerDetails = await _dbcontext.EthernetLapRF8Channels
            .Select(x => new Messages.Models.EthernetLapRF8ChannelTimer(x.Id, x.Name, new IPAddress(x.IpAddress), x.Port))
            .ToListAsync().ConfigureAwait(false);

        return timerDetails;
    }
    public async Task<Messages.Models.EthernetLapRF8ChannelTimer?> GetEthernetLapRF8ChannelTimer(Guid timerId)
    {
        var timer = await _dbcontext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == timerId).ConfigureAwait(false);

        return timer == null ? null : new Messages.Models.EthernetLapRF8ChannelTimer(timer.Id, timer.Name, new IPAddress(timer.IpAddress), timer.Port);
    }
}
