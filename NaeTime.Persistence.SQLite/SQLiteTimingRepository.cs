using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.SQLite.Context;
using NaeTime.Persistence.SQLite.Models;

namespace NaeTime.Persistence.SQLite;
public class SQLiteTimingRepository : ITimingRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteTimingRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task AddManualDetection(Guid timerId, byte lane, long softwareTime, DateTime utcTime)
    {
        var detection = new Detection
        {
            Id = Guid.NewGuid(),
            TimerId = timerId,
            Lane = lane,
            SoftwareTime = softwareTime,
            UtcTime = utcTime,
            IsManual = true
        };

        _dbContext.Detections.Add(detection);

        return _dbContext.SaveChangesAsync();
    }
    public Task AddTimerDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var detection = new Detection
        {
            Id = Guid.NewGuid(),
            TimerId = timerId,
            Lane = lane,
            HardwareTime = hardwareTime,
            SoftwareTime = softwareTime,
            UtcTime = utcTime,
            IsManual = false
        };

        _dbContext.Detections.Add(detection);

        return _dbContext.SaveChangesAsync();
    }
    public async Task SetLanePilot(byte lane, Guid? pilotId)
    {
        var existingStatus = await _dbContext.Lanes.FirstOrDefaultAsync(x => x.Id == lane);

        if (existingStatus == null)
        {
            existingStatus = new Lane
            {
                Id = lane,
                PilotId = pilotId,
                IsEnabled = true,
            };

            _dbContext.Lanes.Add(existingStatus);
        }
        else
        {
            existingStatus.PilotId = pilotId;
        }

        await _dbContext.SaveChangesAsync();
    }
    public async Task SetLaneRadioFrequency(byte lane, int frequencyInMhz)
    {
        var existingStatus = await _dbContext.Lanes.FirstOrDefaultAsync(x => x.Id == lane);

        if (existingStatus == null)
        {
            existingStatus = new Lane
            {
                Id = lane,
                RadioFrequencyInMhz = frequencyInMhz,
                IsEnabled = true,
            };

            _dbContext.Lanes.Add(existingStatus);
        }
        else
        {
            existingStatus.RadioFrequencyInMhz = frequencyInMhz;
        }

        await _dbContext.SaveChangesAsync();
    }
    public Task SetLaneStatus(byte lane, bool isEnabled)
    {
        var existingStatus = _dbContext.Lanes.FirstOrDefault(x => x.Id == lane);

        if (existingStatus == null)
        {
            existingStatus = new Lane
            {
                Id = lane,
                IsEnabled = isEnabled
            };

            _dbContext.Lanes.Add(existingStatus);
        }
        else
        {
            existingStatus.IsEnabled = isEnabled;
        }

        return _dbContext.SaveChangesAsync();
    }
}
