using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteActiveRepository : IActiveRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLiteActiveRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task ActivateSession(Guid sessionId, SessionType type)
    {
        var existingEntities = await _dbContext.ActiveSession.FirstOrDefaultAsync().ConfigureAwait(false);

        if (existingEntities != null)
        {
            _ = _dbContext.ActiveSession.Remove(existingEntities);
        }

        var newActiveSession = new Models.ActiveSession
        {
            Id = Guid.NewGuid(),
            SessionId = sessionId,
            SessionType = type switch
            {
                SessionType.OpenPractice => Models.SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            },
        };

        _ = _dbContext.ActiveSession.Add(newActiveSession);

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task DeactivateSession()
    {
        var existingEntities = await _dbContext.ActiveSession.FirstOrDefaultAsync().ConfigureAwait(false);

        if (existingEntities == null)
        {
            return;
        }

        _ = _dbContext.ActiveSession.Remove(existingEntities);

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task ActivateLap(Guid sessionId, byte lane, uint lapNumber, long startedSoftwareTime, DateTime startedUtcTime, ulong? startedHardwareTime)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new Models.ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = lane,
                SessionId = sessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = lapNumber;

        var activeLap = new Models.ActiveLap
        {
            Id = Guid.NewGuid(),
            ActiveTimingsId = existingTimings.Id,
            StartedSoftwareTime = startedSoftwareTime,
            StartedUtcTime = startedUtcTime,
            StartedHardwareTime = startedHardwareTime
        };

        existingTimings.ActiveLap = activeLap;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);

    }
    public async Task DeactivateLap(Guid sessionId, byte lane)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveLap = null;

        _ = _dbContext.ActiveTimings.Update(existingTimings);

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task ActivateSplit(Guid sessionId, byte lane, uint lapNumber, byte splitNumber, long startedSoftwareTime, DateTime startedUtcTime)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new Models.ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = lane,
                SessionId = sessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = lapNumber;

        var activeSplit = new Models.ActiveSplit
        {
            Id = Guid.NewGuid(),
            ActiveTimingsId = existingTimings.Id,
            SplitNumber = splitNumber,
            StartedSoftwareTime = startedSoftwareTime,
            StartedUtcTime = startedUtcTime
        };

        existingTimings.ActiveSplit = activeSplit;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task DeactivateSplit(Guid sessionId, byte lane)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveSplit = null;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<ActiveSession?> GetSession()
    {
        var existingSession = await _dbContext.ActiveSession.FirstOrDefaultAsync().ConfigureAwait(false);

        return existingSession == null
            ? null
            : new ActiveSession(existingSession.SessionId, existingSession.SessionType switch
            {
                Models.SessionType.OpenPractice => SessionType.OpenPractice,
                _ => throw new NotImplementedException()
            });
    }

    public async Task<ActiveTimings> GetTimings(Guid trackId, byte lane)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == trackId && x.Lane == lane).ConfigureAwait(false)
            ?? new Models.ActiveTimings()
            {
                Id = Guid.NewGuid(),
                SessionId = trackId,
                Lane = lane,
            };

        ActiveLap? activeLap = null;

        if (existingTimings.ActiveLap != null)
        {
            var lap = existingTimings.ActiveLap;
            activeLap = new ActiveLap(lap.StartedSoftwareTime, lap.StartedUtcTime, lap.StartedHardwareTime);
        }

        ActiveSplit? activeSplit = null;

        if (existingTimings.ActiveSplit != null)
        {
            var split = existingTimings.ActiveSplit;
            activeSplit = new ActiveSplit(split.SplitNumber, split.StartedSoftwareTime, split.StartedUtcTime);
        }

        return new ActiveTimings(trackId, lane, existingTimings.LapNumber, activeLap, activeSplit);
    }

    public async Task<IEnumerable<ActiveTimings>> GetTimings(Guid sessionId)
    {
        var timings = await _dbContext.ActiveTimings.ToListAsync().ConfigureAwait(false);

        return timings.Where(x => x.SessionId == sessionId).Select(x =>
        new ActiveTimings(x.SessionId, x.Lane, x.LapNumber, x.ActiveLap != null ? new ActiveLap(x.ActiveLap.StartedSoftwareTime, x.ActiveLap.StartedUtcTime, x.ActiveLap.StartedHardwareTime) : null, x.ActiveSplit != null ? new ActiveSplit(x.ActiveSplit.SplitNumber, x.ActiveSplit.StartedSoftwareTime, x.ActiveSplit.StartedUtcTime) : null));
    }
}
