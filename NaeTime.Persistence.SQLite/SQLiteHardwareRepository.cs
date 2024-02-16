using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteHardwareRepository : IHardwareRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteHardwareRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    private TimerDetails GetTimerDetails(Models.EthernetLapRF8Channel timer)
        => new()
        {
            Id = timer.Id,
            Name = timer.Name,
            Type = TimerType.EthernetLapRF8Channel,
            AllowedLanes = 8,
        };

    public async Task AddOrUpdateEthernetLapRF8Channel(Guid id, string name, byte[] ipAddress, int port)
    {
        var existing = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

        if (existing == null)
        {
            existing = new Models.EthernetLapRF8Channel
            {
                Id = id,
                IpAddress = ipAddress,
                Port = port,
                Name = name,
            };

            _ = _dbContext.EthernetLapRF8Channels.Add(existing);
        }
        else
        {
            existing.IpAddress = ipAddress;
            existing.Port = port;
            existing.Name = name;
        }

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<EthernetLapRF8Channel>> GetAllEthernetLapRF8ChannelTimers() =>
        await _dbContext.EthernetLapRF8Channels.Select(x => new EthernetLapRF8Channel(x.Id, x.Name, x.IpAddress, x.Port)).ToListAsync().ConfigureAwait(false);

    public async Task<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        var timers = new List<TimerDetails>();

        var ethernetLapRF8Channels = await _dbContext.EthernetLapRF8Channels.ToListAsync().ConfigureAwait(false);

        timers.AddRange(ethernetLapRF8Channels.Select(GetTimerDetails));

        return timers;
    }

    public async Task<EthernetLapRF8Channel?> GetEthernetLapRF8Channel(Guid id)
    {
        var timer = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

        return timer == null ? null : new EthernetLapRF8Channel(timer.Id, timer.Name, timer.IpAddress, timer.Port);
    }

    public async Task<IEnumerable<TimerDetails>> GetTimerDetails(IEnumerable<Guid> ids)
    {
        var timers = new List<TimerDetails>();

        var ethernetLapRF8Channels = await _dbContext.EthernetLapRF8Channels.Where(x => ids.Contains(x.Id)).ToListAsync().ConfigureAwait(false);

        timers.AddRange(ethernetLapRF8Channels.Select(GetTimerDetails));

        return timers;
    }

    public async Task SetTimerConnectionStatus(Guid id, bool isConnected, DateTime utcTime)
    {
        var existing = await _dbContext.TimerStatuses.FirstOrDefaultAsync(x => x.Id == id).ConfigureAwait(false);

        if (existing == null)
        {
            existing = new Models.TimerStatus
            {
                ConnectionStatusChanged = utcTime,
                WasConnected = isConnected,
            };

            _ = _dbContext.TimerStatuses.Add(existing);
        }
        else
        {
            existing.ConnectionStatusChanged = utcTime;
            existing.WasConnected = isConnected;
        }

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
