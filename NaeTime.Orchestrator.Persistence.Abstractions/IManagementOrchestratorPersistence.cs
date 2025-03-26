namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface IManagementOrchestratorPersistence
{
    Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign);
    Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign);
    Task ActivateOpenPracticeSession(Guid sessionId);
    Task<Guid> CreateTrack(string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers);
    Task<bool> UpdateTrack(Guid trackId, string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers);
}
