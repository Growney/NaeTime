using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Orchestrator.Abstractions;

public interface IManagementOrchestrator
{
    Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign);
    Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign);
    Task ActivateOpenPracticeSession(Guid sessionId);
    Task<Guid> CreateTrack(string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers);
    Task<bool> UpdateTrack(Guid id, string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers);
    Task<Guid> CreateOpenPracticeSession(string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds);

    public Task<IEnumerable<Pilot>> GetPilots();
    public Task<Pilot?> GetPilot(Guid pilotId);
    public Task<Track?> GetTrack(Guid trackId);
    public Task<IEnumerable<Track>> GetTracks();
    public Task<ActiveSession?> GetActiveSession();
    public Task<int> GetTrackAllowedLanes(Guid trackId);
    public Task<IEnumerable<TimerDetails>> GetTrackTimers(Guid trackId);
}
