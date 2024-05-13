namespace NaeTime.Timing.SQLite;
internal class ActiveTimingService
{
    private readonly TimingDbContext _dbContext;
    public ActiveTimingService(TimingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Messages.Models.LaneActiveTimings?> GetLaneActiveTimings(Guid sessionId, byte lane)
    {
        ActiveTimings? activeTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == lane);
        if (activeTimings == null)
        {
            return null;
        }

        Messages.Models.ActiveLap? lap = null;
        if (activeTimings.ActiveLap != null)
        {
            lap = new Messages.Models.ActiveLap(activeTimings.ActiveLap.StartedSoftwareTime, activeTimings.ActiveLap.StartedUtcTime, activeTimings.ActiveLap.StartedHardwareTime);
        }

        Messages.Models.ActiveSplit? split = null;
        if (activeTimings.ActiveSplit != null)
        {
            split = new Messages.Models.ActiveSplit(activeTimings.ActiveSplit.SplitNumber, activeTimings.ActiveSplit.StartedSoftwareTime, activeTimings.ActiveSplit.StartedUtcTime);
        }

        return new Messages.Models.LaneActiveTimings(activeTimings.Lane, activeTimings.LapNumber, lap, split);
    }

    public async Task<IEnumerable<Messages.Models.LaneActiveTimings>> GetSessionActiveTimings(Guid sessionId)
    {
        List<ActiveTimings> timings = await _dbContext.ActiveTimings.Where(x => x.SessionId == sessionId).ToListAsync();

        List<Messages.Models.LaneActiveTimings> responseData = new();

        foreach (ActiveTimings? timing in timings)
        {
            Messages.Models.ActiveLap? lap = null;
            if (timing.ActiveLap != null)
            {
                lap = new Messages.Models.ActiveLap((long)timing.ActiveLap.StartedSoftwareTime, (DateTime)timing.ActiveLap.StartedUtcTime, (ulong?)timing.ActiveLap.StartedHardwareTime);
            }

            Messages.Models.ActiveSplit? split = null;
            if (timing.ActiveSplit != null)
            {
                split = new Messages.Models.ActiveSplit((byte)timing.ActiveSplit.SplitNumber, (long)timing.ActiveSplit.StartedSoftwareTime, (DateTime)timing.ActiveSplit.StartedUtcTime);
            }

            responseData.Add(new Messages.Models.LaneActiveTimings(timing.Lane, timing.LapNumber, lap, split));
        }

        return responseData;
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
