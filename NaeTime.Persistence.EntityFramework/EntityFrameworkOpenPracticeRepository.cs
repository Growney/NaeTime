using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkOpenPracticeRepository : IOpenPracticeRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public EntityFrameworkOpenPracticeRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);
        var lanes = await _dbContext.OpenPracticeLaneConfigurations.ToListAsync().ConfigureAwait(false);

        if (session == null)
        {
            return null;
        }

        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync().ConfigureAwait(false);

        return new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
                       laps.Select(x => new Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, GetSessionResponseStatus(x.Status), x.TotalMilliseconds)),
                        lanes.Select(y => new OpenPracticeLaneConfiguration(y.Lane, y.PilotId, y.IsEnabled, y.BandId, y.FrequencyInMhz)),
                                             session.TrackedConsecutiveLaps.Select(x => x.LapCap));
    }
    public async Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.AverageLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new AverageLapLeaderboardPosition(x.Position, x.PilotId, x.AverageMilliseconds, x.FirstLapCompletionUtc));
    }
    public async Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap)
    {
        var positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.LapCap == lapCap).ToListAsync();

        return positions.Select(x => new ConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }
    public async Task<Lap?> GetOpenPracticeSessionLap(Guid lapId)
    {
        var lap = await _dbContext.OpenPracticeLaps.Where(x => x.Id == lapId).FirstOrDefaultAsync();

        return lap == null
            ? null
            : new Lap(lap.Id, lap.PilotId, lap.StartedUtc, lap.FinishedUtc, lap.Status switch
            {
                Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
                Models.OpenPracticeLapStatus.Completed => LapStatus.Completed,
                _ => throw new NotImplementedException()
            }, lap.TotalMilliseconds);
    }
    public async Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId)
    {
        List<LapRecord> records = new();
        var singleLapRecord = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId).ConfigureAwait(false);

        if (singleLapRecord != null)
        {
            records.Add(new LapRecord(1, [singleLapRecord.LapId]));
        }

        var consecutiveLapRecords = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync().ConfigureAwait(false);

        if (consecutiveLapRecords.Any())
        {
            records.AddRange(consecutiveLapRecords.Select(x => new LapRecord(x.LapCap, x.IncludedLaps.Select(x => x.LapId))));
        }

        return records;
    }
    public async Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId)
    {
        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync();

        return laps.Select(x => new Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, x.Status switch
        {
            Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
            Models.OpenPracticeLapStatus.Completed => LapStatus.Completed,
            _ => throw new NotImplementedException()
        }, x.TotalMilliseconds));

    }
    public async Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions()
    {
        var sessions = await _dbContext.OpenPracticeSessions.ToListAsync().ConfigureAwait(false);
        var lanes = await _dbContext.OpenPracticeLaneConfigurations.ToListAsync().ConfigureAwait(false);

        List<OpenPracticeSession> responseSessions = new();
        foreach (var session in sessions)
        {
            var laps = await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == session.Id).ToListAsync().ConfigureAwait(false);

            responseSessions.Add(new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds,
                laps.Select(y => new Lap(y.Id, y.PilotId, y.StartedUtc, y.FinishedUtc, GetSessionResponseStatus(y.Status), y.TotalMilliseconds)),
            lanes.Select(y => new OpenPracticeLaneConfiguration(y.Lane, y.PilotId, y.IsEnabled, y.BandId, y.FrequencyInMhz)),
            session.TrackedConsecutiveLaps.Select(y => y.LapCap)));
        }

        return responseSessions;
    }
    private LapStatus GetSessionResponseStatus(Models.OpenPracticeLapStatus status) => status switch
    {
        Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
        Models.OpenPracticeLapStatus.Completed => LapStatus.Completed,
        _ => throw new NotImplementedException()
    };
    public async Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.SingleLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new SingleLapLeaderboardPosition(x.Position, x.PilotId, x.TotalMilliseconds, x.CompletionUtc, x.LapId));
    }
    public async Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.TotalLapsLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new TotalLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.FirstLapCompletionUtc));
    }
    public async Task<AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new AverageLapRecord(position.AverageMilliseconds, position.FirstLapCompletionUtc);
    }

    public async Task<IEnumerable<ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId)
    {
        var positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync();

        return positions.Select(x => new ConsecutiveLapRecord(x.LapCap, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }
    public async Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId)
    {
        var laps = await _dbContext.OpenPracticeLaps.Where(x => x.PilotId == pilotId && x.SessionId == sessionId).ToListAsync();

        return laps.Select(x => new Lap(x.Id, pilotId, x.StartedUtc, x.FinishedUtc, x.Status switch
        {
            Models.OpenPracticeLapStatus.Invalid => LapStatus.Invalid,
            Models.OpenPracticeLapStatus.Completed => LapStatus.Completed,
            _ => throw new NotImplementedException()
        }, x.TotalMilliseconds));
    }

    public async Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        return session == null ? Enumerable.Empty<uint>() : session.TrackedConsecutiveLaps.Select(x => x.LapCap).ToList();
    }

    public async Task<SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new SingleLapRecord(position.TotalMilliseconds, position.CompletionUtc, position.LapId);
    }

    public async Task<TotalLapRecord?> GetPilotOpenPracticeSessionTotalLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.TotalLapsLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new TotalLapRecord(position.TotalLaps, position.FirstLapCompletionUtc);
    }

    public async Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds) => await _dbContext.OpenPracticeLaps.Where(x => lapIds.Contains(x.Id)).Select(x => new Lap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc, GetSessionResponseStatus(x.Status), x.TotalMilliseconds)).ToListAsync();
}
