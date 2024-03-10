namespace NaeTime.Timing.SQLite;
internal class ActiveTimingService : ISubscriber
{
    private readonly TimingDbContext _dbContext;
    public ActiveTimingService(TimingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ActiveTimingResponse?> On(ActiveTimingRequest request)
    {
        var activeTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == request.SessionId && x.Lane == request.Lane);
        if (activeTimings == null)
        {
            return null;
        }

        ActiveTimingResponse.ActiveLap? lap = null;
        if (activeTimings.ActiveLap != null)
        {
            lap = new ActiveTimingResponse.ActiveLap(activeTimings.ActiveLap.StartedSoftwareTime, activeTimings.ActiveLap.StartedUtcTime, activeTimings.ActiveLap.StartedHardwareTime);
        }

        ActiveTimingResponse.ActiveSplit? split = null;
        if (activeTimings.ActiveSplit != null)
        {
            split = new ActiveTimingResponse.ActiveSplit(activeTimings.ActiveSplit.SplitNumber, activeTimings.ActiveSplit.StartedSoftwareTime, activeTimings.ActiveSplit.StartedUtcTime);
        }

        return new ActiveTimingResponse(activeTimings.SessionId, activeTimings.Lane, activeTimings.LapNumber, lap, split);
    }

    public async Task<ActiveTimingsResponse> On(ActiveTimingsRequest request)
    {
        var timings = await _dbContext.ActiveTimings.Where(x => x.SessionId == request.SessionId).ToListAsync();

        var responseData = new List<ActiveTimingsResponse.ActiveTimings>();

        foreach (var timing in timings)
        {
            ActiveTimingsResponse.ActiveLap? lap = null;
            if (timing.ActiveLap != null)
            {
                lap = new ActiveTimingsResponse.ActiveLap(timing.ActiveLap.StartedSoftwareTime, timing.ActiveLap.StartedUtcTime, timing.ActiveLap.StartedHardwareTime);
            }

            ActiveTimingsResponse.ActiveSplit? split = null;
            if (timing.ActiveSplit != null)
            {
                split = new ActiveTimingsResponse.ActiveSplit(timing.ActiveSplit.SplitNumber, timing.ActiveSplit.StartedSoftwareTime, timing.ActiveSplit.StartedUtcTime);
            }

            responseData.Add(new ActiveTimingsResponse.ActiveTimings(timing.Lane, timing.LapNumber, lap, split));
        }

        return new ActiveTimingsResponse(request.SessionId, responseData);
    }

    public async Task When(LapCompleted completed)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == completed.SessionId && x.Lane == completed.Lane).ConfigureAwait(false);

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
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == invalidated.SessionId && x.Lane == invalidated.Lane).ConfigureAwait(false);

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
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == started.SessionId && x.Lane == started.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new Models.ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = started.Lane,
                SessionId = started.SessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = started.LapNumber;

        var activeLap = new Models.ActiveLap
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
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == completed.SessionId && x.Lane == completed.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveSplit = null;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(SplitStarted started)
    {
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == started.SessionId && x.Lane == started.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            existingTimings = new Models.ActiveTimings()
            {
                Id = Guid.NewGuid(),
                Lane = started.Lane,
                SessionId = started.SessionId
            };
            _ = _dbContext.ActiveTimings.Add(existingTimings);
        }

        existingTimings.LapNumber = started.LapNumber;

        var activeSplit = new Models.ActiveSplit
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
        var existingTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == skipped.SessionId && x.Lane == skipped.Lane).ConfigureAwait(false);

        if (existingTimings == null)
        {
            return;
        }

        existingTimings.ActiveSplit = null;

        _ = await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
