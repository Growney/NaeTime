using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteOpenPracticeSessionRepository : IOpenPracticeSessionRepository
{
    private readonly NaeTimeDbContext _dbContext;
    public SQLiteOpenPracticeSessionRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public async Task AddLapToSession(Guid sessionId, Guid lapId, Guid pilotId, OpenPracticeLapStatus status, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds)
    {
        var lap = new Models.OpenPracticeLap
        {
            Id = lapId,
            SessionId = sessionId,
            PilotId = pilotId,
            Status = status switch
            {
                OpenPracticeLapStatus.Invalid => Models.OpenPracticeLapStatus.Invalid,
                OpenPracticeLapStatus.Completed => Models.OpenPracticeLapStatus.Completed,
                _ => throw new NotImplementedException()
            },
            StartedUtc = startedUtc,
            FinishedUtc = finishedUtc,
            TotalMilliseconds = totalMilliseconds
        };

        _dbContext.OpenPracticeLaps.Add(lap);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task UpdateConsecutiveLapsLeaderboardPositions(Guid sessionId, Guid leaderboardId, IEnumerable<ConsecutiveLapLeaderboardPosition> positions)
    {

        var existingLeaderboard = await _dbContext.ConsecutiveLapLeaderboards.FirstOrDefaultAsync(x => x.Id == leaderboardId).ConfigureAwait(false);

        if (existingLeaderboard == null)
        {
            return;
        }

        existingLeaderboard.Positions.Clear();

        existingLeaderboard.Positions.AddRange(positions.Select(x => new Models.ConsecutiveLapLeaderboardPosition
        {
            Id = Guid.NewGuid(),
            Position = x.Position,
            PilotId = x.PilotId,
            TotalLaps = x.TotalLaps,
            TotalMilliseconds = x.TotalMilliseconds,
            LastLapCompletionUtc = x.LastLapCompletionUtc,
            IncludedLaps = x.IncludedLaps.Select(lapId => new Models.ConsecutiveLapLeaderboardPositionLap
            {
                Id = Guid.NewGuid(),
                LapId = lapId,
                //This feels bad but in order to do it on a single line its requried
                Ordinal = x.IncludedLaps.TakeWhile(y => y != lapId).Count()
            }).ToList()
        }));

        _dbContext.ConsecutiveLapLeaderboards.Update(existingLeaderboard);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task UpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId, IEnumerable<SingleLapLeaderboardPosition> positions)
    {
        var existingLeaderboard = await _dbContext.SingleLapLeaderboards.FirstOrDefaultAsync(x => x.Id == leaderboardId).ConfigureAwait(false);

        if (existingLeaderboard == null)
        {
            return;
        }

        existingLeaderboard.Positions.Clear();

        existingLeaderboard.Positions.AddRange(positions.Select(x => new Models.SingleLapLeaderboardPosition
        {
            Id = Guid.NewGuid(),
            Position = x.Position,
            PilotId = x.PilotId,
            LapId = x.LapId,
            CompletionUtc = x.CompletionUtc,
            LapMilliseconds = x.LapMilliseconds,
        }));

        _dbContext.SingleLapLeaderboards.Update(existingLeaderboard);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<OpenPracticeSession?> Get(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions
            .Include(x => x.ActiveLanes).FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return null;
        }
        //If we don't call to list on each of these, some sort of magic happens and the lists are bound to the database so calls that don't expect them to have been updated end up being updated,
        //look at turning off tracking and update the rest of the update queries to update records
        var lanes = session.ActiveLanes.Select(x => new PilotLane(x.PilotId, x.Lane)).ToList();
        var laps = (await _dbContext.OpenPracticeLaps.Where(x => x.SessionId == sessionId).ToListAsync().ConfigureAwait(false)).Select(x => new OpenPracticeLap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                Models.OpenPracticeLapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                Models.OpenPracticeLapStatus.Completed => OpenPracticeLapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds));

        var singleLapLeaderboards = await _dbContext.SingleLapLeaderboards.Where(x => x.SessionId == sessionId).Select(
            x => new SingleLapLeaderboard(x.Id,
                x.Positions.Select(y => new SingleLapLeaderboardPosition(y.Position, y.PilotId, y.LapId, y.LapMilliseconds, y.CompletionUtc)))).ToListAsync();

        var consecutiveLapLeaderboards = await _dbContext.ConsecutiveLapLeaderboards.Where(x => x.SessionId == sessionId).Select(
            x => new ConsecutiveLapLeaderboard(x.Id, x.ConsecutiveLaps,
                x.Positions.Select(y => new ConsecutiveLapLeaderboardPosition(y.Position, y.PilotId, y.TotalLaps, y.TotalMilliseconds, y.LastLapCompletionUtc, y.IncludedLaps.OrderBy(x => x.Ordinal).Select(x => x.LapId))))).ToListAsync();

        return new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, laps, lanes, singleLapLeaderboards, consecutiveLapLeaderboards);
    }
    public async Task SetSessionLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var existingLane = session.ActiveLanes.FirstOrDefault(x => x.Lane == lane);

        if (existingLane == null)
        {
            existingLane = new Models.PilotLane()
            {
                Id = Guid.NewGuid(),
                Lane = lane,
            };
            session.ActiveLanes.Add(existingLane);
        }
        existingLane.PilotId = pilotId;

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task AddOrUpdateConsecutiveLapsLeaderboard(Guid sessionId, Guid leaderboardId, uint consecutiveLaps)
    {
        var existingLeaderboard = await _dbContext.ConsecutiveLapLeaderboards.FirstOrDefaultAsync(x => x.Id == leaderboardId && x.SessionId == sessionId).ConfigureAwait(false);

        if (existingLeaderboard == null)
        {
            existingLeaderboard = new Models.ConsecutiveLapLeaderboard
            {
                Id = leaderboardId,
                SessionId = sessionId,
            };
            _dbContext.ConsecutiveLapLeaderboards.Add(existingLeaderboard);
        }

        existingLeaderboard.ConsecutiveLaps = consecutiveLaps;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task AddOrUpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId)
    {
        var existingLeaderboard = await _dbContext.SingleLapLeaderboards.FirstOrDefaultAsync(x => x.Id == leaderboardId && x.SessionId == sessionId);

        //Because there is nothing to update on this we just return 
        if (existingLeaderboard != null)
        {
            return;
        }

        existingLeaderboard = new Models.SingleLapLeaderboard
        {
            Id = leaderboardId,
            SessionId = sessionId,
        };

        _dbContext.SingleLapLeaderboards.Add(existingLeaderboard);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task RemoveLeaderboard(Guid sessionId, Guid leaderboardId)
    {
        await _dbContext.SingleLapLeaderboards.Where(x => x.SessionId == sessionId && x.Id == leaderboardId).ExecuteDeleteAsync();
        await _dbContext.ConsecutiveLapLeaderboards.Where(x => x.SessionId == sessionId && x.Id == leaderboardId).ExecuteDeleteAsync();
    }

    public async Task AddOrUpdate(Guid sessionId, string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (existing == null)
        {
            existing = new Models.OpenPracticeSession
            {
                Id = sessionId,
            };
            _dbContext.OpenPracticeSessions.Add(existing);
        }
        existing.TrackId = trackId;
        existing.MinimumLapMilliseconds = minimumLapMilliseconds;
        existing.MaximumLapMilliseconds = maximumLapMilliseconds;
        existing.Name = name;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public Task RemoveLap(Guid sessionId, Guid lapId) => _dbContext.OpenPracticeLaps.Where(x => x.Id == lapId && x.SessionId == sessionId).ExecuteDeleteAsync();


    public async Task SetMinimumLap(Guid sessionId, long minimumLapMilliseconds)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MinimumLapMilliseconds = minimumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task SetMaximumLap(Guid sessionId, long? maximumLapMilliseconds)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId).ConfigureAwait(false);

        if (existing == null)
        {
            return;
        }

        existing.MaximumLapMilliseconds = maximumLapMilliseconds;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
