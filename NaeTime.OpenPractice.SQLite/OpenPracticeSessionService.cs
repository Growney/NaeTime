using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class OpenPracticeSessionService : ISubscriber
{
    private readonly OpenPracticeDbContext _dbContext;
    public OpenPracticeSessionService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(OpenPracticeLapDisputed lap)
    {
        var existing = await _dbContext.OpenPracticeLaps.FirstOrDefaultAsync(x => x.Id == lap.LapId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.Status = lap.ActualStatus switch
        {
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Invalid => OpenPracticeLapStatus.Invalid,
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed => OpenPracticeLapStatus.Completed,
            _ => throw new NotImplementedException()
        };

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapRemoved removed)
    {
        var existing = await _dbContext.OpenPracticeLaps.FirstOrDefaultAsync(x => x.Id == removed.LapId).ConfigureAwait(false);
        if (existing == null)
        {
            return;
        }

        _dbContext.OpenPracticeLaps.Remove(existing);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeSessionConfigured configured)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

        if (existing == null)
        {
            existing = new OpenPracticeSession
            {
                Id = configured.SessionId,
            };
            _dbContext.OpenPracticeSessions.Add(existing);
        }

        existing.Name = configured.Name;
        existing.TrackId = configured.TrackId;
        existing.MinimumLapMilliseconds = configured.MinimumLapMilliseconds;
        existing.MaximumLapMilliseconds = configured.MaximumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMaximumLapTimeConfigured configured)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MaximumLapMilliseconds = configured.MaximumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeMinimumLapTimeConfigured configured)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MinimumLapMilliseconds = configured.MinimumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapCompleted openPracticeLapCompleted)
    {
        _dbContext.OpenPracticeLaps.Add(new OpenPracticeLap
        {
            Id = openPracticeLapCompleted.LapId,
            SessionId = openPracticeLapCompleted.SessionId,
            PilotId = openPracticeLapCompleted.PilotId,
            Status = OpenPracticeLapStatus.Completed,
            StartedUtc = openPracticeLapCompleted.StartedUtc,
            FinishedUtc = openPracticeLapCompleted.FinishedUtc,
            TotalMilliseconds = openPracticeLapCompleted.TotalMilliseconds
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLapInvalidated openPracticeLapInvalidated)
    {
        _dbContext.OpenPracticeLaps.Add(new OpenPracticeLap
        {
            Id = openPracticeLapInvalidated.LapId,
            SessionId = openPracticeLapInvalidated.SessionId,
            PilotId = openPracticeLapInvalidated.PilotId,
            Status = OpenPracticeLapStatus.Invalid,
            StartedUtc = openPracticeLapInvalidated.StartedUtc,
            FinishedUtc = openPracticeLapInvalidated.FinishedUtc,
            TotalMilliseconds = openPracticeLapInvalidated.TotalMilliseconds
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(OpenPracticeLanePilotSet laneSet)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == laneSet.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var existingLane = session.ActiveLanes.FirstOrDefault(x => x.Lane == laneSet.Lane);

        if (existingLane == null)
        {
            existingLane = new PilotLane()
            {
                Id = Guid.NewGuid(),
                Lane = laneSet.Lane,
            };

            session.ActiveLanes.Add(existingLane);
        }

        existingLane.PilotId = laneSet.PilotId;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(ConsecutiveLapCountTracked tracked)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == tracked.SessionId).ConfigureAwait(false);

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
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == removed.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var existing = session.TrackedConsecutiveLaps.FirstOrDefault(x => x.LapCap == removed.LapCap);

        if (existing == null)
        {
            return;
        }

        session.TrackedConsecutiveLaps.Remove(existing);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);

    }

    public async Task<OpenPracticeSessionResponse?> On(OpenPracticeSessionRequest request)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == request.SessionId).ToListAsync().ConfigureAwait(false);


        return session == null ?
            null : new OpenPracticeSessionResponse(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
            laps.Select(x => new OpenPracticeSessionResponse.Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                OpenPracticeLapStatus.Invalid => OpenPracticeSessionResponse.LapStatus.Invalid,
                OpenPracticeLapStatus.Completed => OpenPracticeSessionResponse.LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds)),
            session.ActiveLanes.Select(x => new OpenPracticeSessionResponse.PilotLane(x.PilotId, x.Lane)),
            session.TrackedConsecutiveLaps.Select(x => x.LapCap));

    }
    public async Task<TrackedConsecutiveLapsResponse> On(TrackedConsecutiveLapsRequest request)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == request.SessionId).ConfigureAwait(false);

        List<uint> tracked = new();

        if (session == null)
        {
            return new TrackedConsecutiveLapsResponse(request.SessionId, tracked);
        }

        tracked = session.TrackedConsecutiveLaps.Select(x => x.LapCap).ToList();

        return new TrackedConsecutiveLapsResponse(request.SessionId, tracked);
    }
}

