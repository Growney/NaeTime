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
        var trackedConsecutiveLaps = session.TrackedConsecutiveLaps.Select(x => x.LapCap).ToList();

        return new OpenPracticeSession(session.Id, session.TrackId, session.Name, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, laps, lanes, trackedConsecutiveLaps);
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
    public async Task SetLapStatus(Guid lapId, OpenPracticeLapStatus status)
    {
        var existingLap = await _dbContext.OpenPracticeLaps.FirstOrDefaultAsync(x => x.Id == lapId).ConfigureAwait(false);

        if (existingLap == null)
        {
            return;
        }

        existingLap.Status = status switch
        {
            OpenPracticeLapStatus.Invalid => Models.OpenPracticeLapStatus.Invalid,
            OpenPracticeLapStatus.Completed => Models.OpenPracticeLapStatus.Completed,
            _ => throw new NotImplementedException()
        };

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task AddTrackedConsecutiveLaps(Guid sessionId, uint lapCap)
    {
        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null)
        {
            return;
        }

        if (session.TrackedConsecutiveLaps.Any(x => x.LapCap == lapCap))
        {
            return;
        }

        session.TrackedConsecutiveLaps.Add(new Models.TrackedConsecutiveLaps
        {
            Id = Guid.NewGuid(),
            LapCap = lapCap
        });

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task RemoveTrackedConsecutiveLaps(Guid sessionId, uint lapCap)
    {

        var session = await _dbContext.OpenPracticeSessions.FirstOrDefaultAsync(x => x.Id == sessionId);
        if (session == null)
        {
            return;
        }


        var trackedLap = session.TrackedConsecutiveLaps.FirstOrDefault(x => x.LapCap == lapCap);
        if (trackedLap == null)
        {
            return;
        }

        session.TrackedConsecutiveLaps.Remove(trackedLap);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
