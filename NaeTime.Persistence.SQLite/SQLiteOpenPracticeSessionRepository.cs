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

        var session = _dbContext.OpenPracticeSessions.Include(x => x.Laps).FirstOrDefault(x => x.Id == sessionId);

        if (session != null)
        {
            session.Laps.Add(lap);

            _dbContext.OpenPracticeSessions.Update(session);

            await _dbContext.SaveChangesAsync();
        }
    }
    public async Task UpdateConsecutiveLapsLeaderboardPositions(Guid sessionId, Guid leaderboardId, IEnumerable<ConsecutiveLapLeaderboardPosition> positions)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var existingLeaderboard = session.ConsecutiveLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

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
            IncludedLaps = x.IncludedLaps.Select(x => new Models.ConsecutiveLapLeaderboardPositionLap
            {
                Id = Guid.NewGuid(),
                LapId = x
            }).ToList()
        }));

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync();
    }
    public async Task UpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId, IEnumerable<SingleLapLeaderboardPosition> positions)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var existingLeaderboard = session.SingleLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

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

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync();
    }

    public async Task<OpenPracticeSession?> Get(Guid sessionId)
    {
        var session = await _dbContext.OpenPracticeSessions
            .Include(x => x.ActiveLanes)
            .Include(x => x.Laps)
            .Include(x => x.SingleLapLeaderboards).ThenInclude(x => x.Positions)
            .Include(x => x.ConsecutiveLapLeaderboards).ThenInclude(x => x.Positions).FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return null;
        }
        //If we don't call to list on each of these, some sort of magic happens and the lists are bound to the database so calls that don't expect them to have been updated end up being updated,
        //look at turning off tracking and update the rest of the update queries to update records
        var lanes = session.ActiveLanes.Select(x => new PilotLane(x.PilotId, x.Lane)).ToList();
        var laps = session.Laps.Select(x => new OpenPracticeLap(x.Id, x.PilotId, x.StartedUtc, x.FinishedUtc,
            x.Status switch
            {
                Models.OpenPracticeLapStatus.Invalid => OpenPracticeLapStatus.Invalid,
                Models.OpenPracticeLapStatus.Completed => OpenPracticeLapStatus.Completed,
                _ => throw new NotImplementedException()
            }, x.TotalMilliseconds)).ToList();
        var singleLapLeaderboards = session.SingleLapLeaderboards.Select(
            x => new SingleLapLeaderboard(x.Id,
                x.Positions.Select(y => new SingleLapLeaderboardPosition(y.Position, y.PilotId, y.LapId, y.LapMilliseconds, y.CompletionUtc)))).ToList();
        var consecutiveLapLeaderboards = session.ConsecutiveLapLeaderboards.Select(
            x => new ConsecutiveLapLeaderboard(x.Id, x.ConsecutiveLaps,
                x.Positions.Select(y => new ConsecutiveLapLeaderboardPosition(y.Position, y.PilotId, y.TotalLaps, y.TotalMilliseconds, y.LastLapCompletionUtc, y.IncludedLaps.Select(x => x.LapId))))).ToList();

        return new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, laps, lanes, singleLapLeaderboards, consecutiveLapLeaderboards);
    }
    public async Task SetSessionLanePilot(Guid sessionId, byte lane, Guid pilotId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

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

        await _dbContext.SaveChangesAsync();
    }
    public async Task AddOrUpdateConsecutiveLapsLeaderboard(Guid sessionId, Guid leaderboardId, uint consecutiveLaps)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var existingLeaderboard = session.ConsecutiveLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

        if (existingLeaderboard == null)
        {
            existingLeaderboard = new Models.ConsecutiveLapLeaderboard
            {
                Id = leaderboardId,
            };
            session.ConsecutiveLapLeaderboards.Add(existingLeaderboard);
        }

        existingLeaderboard.ConsecutiveLaps = consecutiveLaps;

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync();
    }
    public async Task AddOrUpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var existingLeaderboard = session.SingleLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

        //Because there is nothing to update on this we just return 
        if (existingLeaderboard != null)
        {
            return;
        }

        existingLeaderboard = new Models.SingleLapLeaderboard
        {
            Id = leaderboardId,
        };
        session.SingleLapLeaderboards.Add(existingLeaderboard);

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync();
    }
    public async Task RemoveLeaderboard(Guid sessionId, Guid leaderboardId)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var singleLapLeaderboard = session.SingleLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

        //Because there is nothing to update on this we just return 
        if (singleLapLeaderboard != null)
        {
            session.SingleLapLeaderboards.Remove(singleLapLeaderboard);
        }

        var consecutiveLapLeaderboard = session.ConsecutiveLapLeaderboards.FirstOrDefault(x => x.Id == leaderboardId);

        if (consecutiveLapLeaderboard != null)
        {
            session.ConsecutiveLapLeaderboards.Remove(consecutiveLapLeaderboard);
        }

        _dbContext.OpenPracticeSessions.Update(session);

        await _dbContext.SaveChangesAsync();
    }

    public async Task AddOrUpdate(Guid sessionId, string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {
        var existing = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);

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

        await _dbContext.SaveChangesAsync();
    }

    public async Task RemoveLap(Guid sessionId, Guid lapId)
    {
        var session = await _dbContext.OpenPracticeSessions.Include(x => x.Laps).FirstOrDefaultAsync(x => x.Id == sessionId);

        if (session == null)
        {
            return;
        }

        var lapIndex = session.Laps.FindIndex(x => x.Id == lapId);
        if (lapIndex == -1)
        {
            return;
        }
        session.Laps.RemoveAt(lapIndex);

        await _dbContext.SaveChangesAsync();
    }
}
