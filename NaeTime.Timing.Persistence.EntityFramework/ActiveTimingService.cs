namespace NaeTime.Timing.Persistence.EntityFramework;
internal class ActiveTimingService
{
    private readonly NaeTimeDbContext _dbContext;
    public ActiveTimingService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(LapCompleted completed)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == completed.SessionId && x.Lane == completed.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveLap = null;

        _dbContext.ActiveTimings.Update(existingTimings);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(LapInvalidated invalidated)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == invalidated.SessionId && x.Lane == invalidated.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveLap = null;

        _dbContext.ActiveTimings.Update(existingTimings);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(LapStarted started)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == started.SessionId && x.Lane == started.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = started.Lane,
                SessionId = started.SessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = started.LapNumber;

        ActiveLap activeLap = new()
        {
            Id = Guid.NewGuid(),
            ActiveTimingsId = existingTimings.Id,
            StartedSoftwareTime = started.StartedSoftwareTime,
            StartedUtcTime = started.StartedUtcTime,
            StartedHardwareTime = started.StartedHardwareTime
        };

        existingTimings.ActiveLap = activeLap;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(SplitCompleted completed)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == completed.SessionId && x.Lane == completed.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveSplit = null;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(SplitStarted started)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == started.SessionId && x.Lane == started.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = started.Lane,
                SessionId = started.SessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = started.LapNumber;

        ActiveSplit activeSplit = new()
        {
            Id = Guid.NewGuid(),
            ActiveTimingsId = existingTimings.Id,
            SplitNumber = started.Split,
            StartedSoftwareTime = started.StartedSoftwareTime,
            StartedUtcTime = started.StartedUtcTime
        };

        existingTimings.ActiveSplit = activeSplit;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(SplitSkipped skipped)
    {
        ActiveTimings? existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == skipped.SessionId && x.Lane == skipped.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveSplit = null;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
