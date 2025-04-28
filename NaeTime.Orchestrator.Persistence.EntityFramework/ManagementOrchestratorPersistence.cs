using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.Persistence.EntityFramework;

namespace NaeTime.Orchestrator.Persistence.EntityFramework;

public class ManagementOrchestratorPersistence : IManagementOrchestratorPersistence
{
    private readonly NaeTimeDbContext _dbContext;

    public ManagementOrchestratorPersistence(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task ActivateOpenPracticeSession(Guid sessionId)
    {
        NaeTime.Persistence.EntityFramework.Models.ActiveSession? active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

        if (active == null)
        {
            active = new NaeTime.Persistence.EntityFramework.Models.ActiveSession
            {
                Id = Guid.NewGuid(),
            };
            _dbContext.ActiveSession.Add(active);
        }

        active.SessionId = sessionId;
        active.SessionType = NaeTime.Persistence.EntityFramework.Models.SessionType.OpenPractice;
    }
    public Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign)
    {
        NaeTime.Persistence.EntityFramework.Models.Pilot pilot = new()
        {
            Id = Guid.NewGuid(),
            FirstName = firstname,
            LastName = lastname,
            CallSign = callsign,
        };
        _dbContext.Pilots.Add(pilot);
        return Task.FromResult(pilot.Id);
    }
    public Task<Guid> CreateTrack(string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers)
    {
        NaeTime.Persistence.EntityFramework.Models.Track track = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            MinimumLapMilliseconds = MinimumLapMilliseconds,
            MaximumLapMilliseconds = MaximumLapMilliseconds,
            Timers = timers.Select((timerId, index) => new NaeTime.Persistence.EntityFramework.Models.TrackTimer()
            {
                Id = Guid.NewGuid(),
                TimerId = timerId,
                OrdinalPosition = index,
            }).ToList(),
        };
        _dbContext.Tracks.Add(track);
        return Task.FromResult(track.Id);
    }
    public Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign)
    {
        NaeTime.Persistence.EntityFramework.Models.Pilot? pilot = _dbContext.Pilots.Find(pilotId);
        if (pilot == null)
        {
            return Task.FromResult(false);
        }

        pilot.FirstName = firstname;
        pilot.LastName = lastname;
        pilot.CallSign = callsign;
        return Task.FromResult(true);
    }

    public Task<bool> UpdateTrack(Guid trackId, string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers)
    {
        NaeTime.Persistence.EntityFramework.Models.Track? track = _dbContext.Tracks.FirstOrDefault(x => x.Id == trackId);
        if (track == null)
        {
            return Task.FromResult(false);
        }

        track.Name = name;
        track.MinimumLapMilliseconds = MinimumLapMilliseconds;
        track.MaximumLapMilliseconds = MaximumLapMilliseconds;
        track.Timers = timers.Select((timerId, index) => new NaeTime.Persistence.EntityFramework.Models.TrackTimer()
        {
            Id = Guid.NewGuid(),
            TimerId = timerId,
            TrackId = trackId,
            OrdinalPosition = index,
        }).ToList();

        return Task.FromResult(true);
    }

    public Task<Guid> CreateOpenPracticeSession(string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {
        NaeTime.Persistence.EntityFramework.Models.OpenPracticeSession session = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            TrackId = trackId,
            MinimumLapMilliseconds = minimumLapMilliseconds,
            MaximumLapMilliseconds = maximumLapMilliseconds,
        };
        _dbContext.OpenPracticeSessions.Add(session);
        return Task.FromResult(session.Id);
    }

    public async Task<ActiveSession?> GetActiveSession()
    {
        var active = await _dbContext.ActiveSession.FirstOrDefaultAsync();
        if (active == null)
        {
            return null;
        }

        return new ActiveSession(active.SessionId, active.SessionType switch
        {
            NaeTime.Persistence.EntityFramework.Models.SessionType.OpenPractice => ActiveSession.SessionType.OpenPractice,
            _ => throw new NotImplementedException()
        });
    }

    public async Task<Pilot?> GetPilot(Guid pilotId)
    {
        var pilot = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == pilotId).ConfigureAwait(false);

        return pilot == null ? null : new Pilot(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }
    public async Task<IEnumerable<Pilot>> GetPilots() => await _dbContext.Pilots.Select(x => new Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)).ToListAsync();
    public async Task<Track?> GetTrack(Guid trackId)
    {
        var track = await _dbContext.Tracks.FirstOrDefaultAsync(x => x.Id == trackId).ConfigureAwait(false);

        return track == null
            ? null
            : new Track(track.Id, track.Name, track.MinimumLapMilliseconds, track.MaximumLapMilliseconds, track.Timers.OrderBy(x => x.OrdinalPosition).Select(x => x.TimerId).ToList());
    }
    public async Task<IEnumerable<Track>> GetTracks() => await _dbContext.Tracks.Select(x => new Track(x.Id, x.Name, x.MinimumLapMilliseconds, x.MaximumLapMilliseconds, x.Timers.OrderBy(y => y.OrdinalPosition).Select(y => y.TimerId).ToList())).ToListAsync();

}
