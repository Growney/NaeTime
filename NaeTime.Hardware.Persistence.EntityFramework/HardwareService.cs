namespace NaeTime.Hardware.Persistence.EntityFramework;
internal class HardwareService
{
    private readonly NaeTimeDbContext _dbcontext;

    public HardwareService(NaeTimeDbContext dbcontext)
    {
        _dbcontext = dbcontext;
    }

    public async Task When(SerialEsp32NodeConfigured configuredEvent)
    {
        SerialEsp32Node? existingTimer = await _dbcontext.SerialEsp32Nodes.FirstOrDefaultAsync(x => x.Id == configuredEvent.TimerId).ConfigureAwait(false);
        if (existingTimer == null)
        {
            existingTimer = new SerialEsp32Node
            {
                Id = configuredEvent.TimerId,
            };
            _dbcontext.SerialEsp32Nodes.Add(existingTimer);
        }

        existingTimer.Name = configuredEvent.Name;
        existingTimer.Port = configuredEvent.Port;
        await _dbcontext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task When(EthernetLapRF8ChannelConfigured configuredEvent)
    {
        EthernetLapRF8Channel? existingTimer = await _dbcontext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == configuredEvent.TimerId).ConfigureAwait(false);

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
        TimerStatus? existingStatus = await _dbcontext.TimerStatuses.FirstOrDefaultAsync(x => x.Id == connectionEstablishedEvent.TimerId).ConfigureAwait(false);
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
        TimerStatus? existingStatus = await _dbcontext.TimerStatuses.FirstOrDefaultAsync(x => x.Id == disconnectedEvent.TimerId).ConfigureAwait(false);
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
}
