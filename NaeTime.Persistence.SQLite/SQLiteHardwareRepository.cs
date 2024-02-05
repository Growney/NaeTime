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

    public async Task AddOrUpdateEthernetLapRF8Channel(Guid id, string name, byte[] ipAddress, int port)
    {
        var existing = await _dbContext.EthernetLapRF8Channels.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            existing = new Models.EthernetLapRF8Channel
            {
                Id = id,
                IpAddress = ipAddress,
                Port = port,
                Name = name,
            };

            _dbContext.EthernetLapRF8Channels.Add(existing);
        }
        else
        {
            existing.IpAddress = ipAddress;
            existing.Port = port;
            existing.Name = name;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TimerDetails>> GetAllTimerDetails()
    {
        var timers = new List<TimerDetails>();

        var ethernetLapRF8Channels = await _dbContext.EthernetLapRF8Channels.ToListAsync();

        timers.AddRange(ethernetLapRF8Channels.Select(x => new TimerDetails
        {
            Id = x.Id,
            Name = x.Name,
            Type = TimerType.EthernetLapRF8Channel
        }));

        return timers;
    }

    public async Task SetTimerConnectionStatus(Guid id, bool isConnected, DateTime utcTime)
    {
        var existing = await _dbContext.TimerStatuses.FirstOrDefaultAsync(x => x.TimerId == id);

        if (existing == null)
        {
            existing = new Models.TimerStatus
            {
                ConnectionStatusChanged = utcTime,
                WasConnected = isConnected,
            };

            _dbContext.TimerStatuses.Add(existing);
        }
        else
        {
            existing.ConnectionStatusChanged = utcTime;
            existing.WasConnected = isConnected;
        }

        await _dbContext.SaveChangesAsync();
    }
}
