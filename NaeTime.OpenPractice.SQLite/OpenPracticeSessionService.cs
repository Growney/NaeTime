using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class OpenPracticeSessionService
{
    private readonly OpenPracticeDbContext _dbContext;
    public OpenPracticeSessionService(OpenPracticeDbContext dbContext)
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
            OpenPracticeLapDisputed.OpenPracticeLapStatus.Completed => OpenPracticeLapStatus.Completed,
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
    public async Task When(OpenPracticeSessionConfigured configured)
    {
        OpenPracticeSession? existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == configured.SessionId).ConfigureAwait(false);

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
        OpenPracticeSession? session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == laneSet.SessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        PilotLane? existingLane = session.ActiveLanes.FirstOrDefault(x => x.Lane == laneSet.Lane);

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
    private Messages.Models.LapStatus GetSessionResponseStatus(OpenPracticeLapStatus status) => status switch
    {
        OpenPracticeLapStatus.Invalid => Messages.Models.LapStatus.Invalid,
        OpenPracticeLapStatus.Completed => Messages.Models.LapStatus.Completed,
        _ => throw new NotImplementedException()
    };
    public async Task<IEnumerable<Messages.Models.OpenPracticeSession>> GetOpenPracticeSessions()
    {
        List<OpenPracticeSession> sessions = await _dbContext.OpenPracticeSessions.ToListAsync().ConfigureAwait(false);

        List<Messages.Models.OpenPracticeSession> responseSessions = new();
        foreach (OpenPracticeSession? session in sessions)
        {
            List<OpenPracticeLap> laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == session.Id).ToListAsync().ConfigureAwait(false);

            responseSessions.Add(new Messages.Models.OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
                laps.Select(y => new Messages.Models.Lap(y.Id, y.PilotId, y.StartedUtc, y.FinishedUtc, GetSessionResponseStatus(y.Status), y.TotalMilliseconds)),
            session.ActiveLanes.Select(y => new Messages.Models.PilotLane(y.PilotId, y.Lane)),
            session.TrackedConsecutiveLaps.Select(y => y.LapCap)));
        }

        return responseSessions;
    }
    public async Task<Messages.Models.OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId)
    {
        OpenPracticeSession? session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        List<OpenPracticeLap> laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync().ConfigureAwait(false);

        return new Messages.Models.OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
                       laps.Select(x => new Messages.Models.Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, GetSessionResponseStatus(x.Status), x.TotalMilliseconds)),
                                  session.ActiveLanes.Select(x => new Messages.Models.PilotLane(x.PilotId, x.Lane)),
                                             session.TrackedConsecutiveLaps.Select(x => x.LapCap));
    }
    public async Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId)
    {
        OpenPracticeSession? session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        return session == null ? Enumerable.Empty<uint>() : session.TrackedConsecutiveLaps.Select(x => x.LapCap).ToList();
    }
    public async Task<IEnumerable<Messages.Models.Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId)
    {
        List<OpenPracticeLap> laps = await _dbContext.OpenPracticeLaps.Where(x => x.PilotId == pilotId && x.SessionId == sessionId).ToListAsync();

        return laps.Select(x => new Messages.Models.Lap(x.Id, pilotId, x.StartedUtc, x.FinishedUtc, x.Status switch
        {
            OpenPracticeLapStatus.Invalid => Messages.Models.LapStatus.Invalid,
            OpenPracticeLapStatus.Completed => Messages.Models.LapStatus.Completed,
            _ => throw new NotImplementedException()
        }, x.TotalMilliseconds));
    }
    public async Task<IEnumerable<Messages.Models.Lap>> GetOpenPracticeSessionLaps(Guid sessionId)
    {
        List<OpenPracticeLap> laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync();

        return laps.Select(x => new Messages.Models.Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, x.Status switch
        {
            OpenPracticeLapStatus.Invalid => Messages.Models.LapStatus.Invalid,
            OpenPracticeLapStatus.Completed => Messages.Models.LapStatus.Completed,
            _ => throw new NotImplementedException()
        }, x.TotalMilliseconds));
    }

    public async Task<Messages.Models.Lap?> GetOpenPracticeSessionLap(Guid lapId)
    {
        OpenPracticeLap? lap = await _dbContext.OpenPracticeLaps.Where(x => x.Id == lapId).FirstOrDefaultAsync();

        return lap == null
            ? null
            : new Messages.Models.Lap(lap.Id, lap.PilotId, lap.StartedUtc, lap.FinishedUtc, lap.Status switch
            {
                OpenPracticeLapStatus.Invalid => Messages.Models.LapStatus.Invalid,
                OpenPracticeLapStatus.Completed => Messages.Models.LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, lap.TotalMilliseconds);
    }

    public async Task<IEnumerable<Messages.Models.Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds)
    {
        List<OpenPracticeLap> laps = await _dbContext.OpenPracticeLaps.Where(x => lapIds.Contains(x.Id)).ToListAsync();

        return laps.Select(x => new Messages.Models.Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, x.Status switch
        {
            OpenPracticeLapStatus.Invalid => Messages.Models.LapStatus.Invalid,
            OpenPracticeLapStatus.Completed => Messages.Models.LapStatus.Completed,
            _ => throw new NotImplementedException()
        }, x.TotalMilliseconds));
    }
}

