namespace NaeTime.OpenPractice.Persistence.EntityFramework;
internal class OpenPracticeSessionService
{
    private readonly NaeTimeDbContext _dbContext;
    public OpenPracticeSessionService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(OpenPracticeLapDisputed lap)
    {
        OpenPracticeLap? existing = await _dbContext.OpenPracticeLaps.FirstOrDefaultAsync(x => x.Id == lap.LapId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.Status = lap.ActualStatus switch
        {
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid => OpenPracticeLapStatus.Invalid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Valid => OpenPracticeLapStatus.Valid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Incomplete => OpenPracticeLapStatus.Incomplete,
            _ => throw new NotImplementedException()
        };

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        OpenPracticeLap? existing = await _dbContext.OpenPracticeLaps.FirstOrDefaultAsync(x => x.Id == removed.LapId).ConfigureAwait(false);
        if (existing == null)
        {
            return;
        }

        _dbContext.OpenPracticeLaps.Remove(existing);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMaximumLapTimeConfigured configured)
    {
        OpenPracticeSession? existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MaximumLapMilliseconds = configured.MaximumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMinimumLapTimeConfigured configured)
    {
        OpenPracticeSession? existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MinimumLapMilliseconds = configured.MinimumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task When(ConsecutiveLapCountTracked tracked)
    {
        OpenPracticeSession? session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == tracked.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        if (session.TrackedConsecutiveLaps.Any(x => x.LapCap == tracked.LapCap))
        {
            return;
        }

        session.TrackedConsecutiveLaps.Add(new TrackedConsecutiveLaps()
        {
            Id = Guid.NewGuid(),
            LapCap = tracked.LapCap
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(ConsecutiveLapCountTrackingRemoved removed)
    {
        OpenPracticeSession? session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == removed.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        TrackedConsecutiveLaps? existing = session.TrackedConsecutiveLaps.FirstOrDefault(x => x.LapCap == removed.LapCap);

        if (existing == null)
        {
            return;
        }

        session.TrackedConsecutiveLaps.Remove(existing);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

    }
}

