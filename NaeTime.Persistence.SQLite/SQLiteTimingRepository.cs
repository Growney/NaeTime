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

}
