using Microsoft.EntityFrameworkCore;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.EntityFramework;
using NaeTime.Persistence.EntityFramework.Models;

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
        ActiveSession? active = await _dbContext.ActiveSession.FirstOrDefaultAsync();

        if (active == null)
        {
            active = new ActiveSession
            {
                Id = Guid.NewGuid(),
            };
            _dbContext.ActiveSession.Add(active);
        }

        active.SessionId = sessionId;
        active.SessionType = SessionType.OpenPractice;
    }
    public Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign)
    {
        Pilot pilot = new()
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
        Track track = new()
        {
            Id = Guid.NewGuid(),
            Name = name,
            MinimumLapMilliseconds = MinimumLapMilliseconds,
            MaximumLapMilliseconds = MaximumLapMilliseconds,
            Timers = timers.Select(x => new TrackTimer()
            {
                Id = Guid.NewGuid(),
                TimerId = x,
            }).ToList(),
        };
        _dbContext.Tracks.Add(track);
        return Task.FromResult(track.Id);
    }
    public Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign)
    {
        Pilot? pilot = _dbContext.Pilots.Find(pilotId);
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
        Track? track = _dbContext.Tracks.Find(trackId);
        if (track == null)
        {
            return Task.FromResult(false);
        }

        track.Name = name;
        track.MinimumLapMilliseconds = MinimumLapMilliseconds;
        track.MaximumLapMilliseconds = MaximumLapMilliseconds;
        track.Timers = timers.Select(x => new TrackTimer()
        {
            Id = Guid.NewGuid(),
            TimerId = x,
            TrackId = trackId
        }).ToList();

        return Task.FromResult(true);
    }
}
