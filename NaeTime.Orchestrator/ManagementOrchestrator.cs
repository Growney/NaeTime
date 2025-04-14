using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Orchestrator;

public class ManagementOrchestrator : IManagementOrchestrator
{
    private readonly IManagementOrchestratorPersistence _managementPersistence;
    private readonly IHardwareOrchestrator _hardwareOrchestrator;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public ManagementOrchestrator(IManagementOrchestratorPersistence managementPersistence, IHardwareOrchestrator hardwareOrchestrator, INaeTimeOrchestratorDistribution distribution)
    {
        _managementPersistence = managementPersistence ?? throw new ArgumentNullException(nameof(managementPersistence));
        _hardwareOrchestrator = hardwareOrchestrator ?? throw new ArgumentNullException(nameof(hardwareOrchestrator));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
    }

    public async Task ActivateOpenPracticeSession(Guid sessionId)
    {
        ActiveSession? currentActiveSession = await _managementPersistence.GetActiveSession();

        if (currentActiveSession != null)
        {
            await _distribution.Distribute(new SessionDeactivated(currentActiveSession.SessionId));
        }

        await _managementPersistence.ActivateOpenPracticeSession(sessionId);
        await _distribution.Distribute(new OpenPracticeSessionActivated(sessionId));
    }

    public Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign) =>
        _managementPersistence.CreatePilot(firstname, lastname, callsign);

    public Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign) =>
        _managementPersistence.UpdatePilot(pilotId, firstname, lastname, callsign);

    public Task<Guid> CreateTrack(string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers) =>
        _managementPersistence.CreateTrack(name, MinimumLapMilliseconds, MaximumLapMilliseconds, timers);

    public Task<bool> UpdateTrack(Guid id, string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers) =>
        _managementPersistence.UpdateTrack(id, name, MinimumLapMilliseconds, MaximumLapMilliseconds, timers);

    public Task<Guid> CreateOpenPracticeSession(string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds) =>
        _managementPersistence.CreateOpenPracticeSession(name, trackId, minimumLapMilliseconds, maximumLapMilliseconds);

    public Task<IEnumerable<Pilot>> GetPilots() =>
        _managementPersistence.GetPilots();

    public Task<Pilot?> GetPilot(Guid pilotId) =>
        _managementPersistence.GetPilot(pilotId);

    public Task<Track?> GetTrack(Guid trackId) =>
        _managementPersistence.GetTrack(trackId);

    public Task<IEnumerable<Track>> GetTracks() =>
        _managementPersistence.GetTracks();

    public Task<ActiveSession?> GetActiveSession() =>
        _managementPersistence.GetActiveSession();
    public async Task<int> GetTrackAllowedLanes(Guid trackId)
    {
        Track? track = await _managementPersistence.GetTrack(trackId);

        if (track == null)
        {
            return 0;
        }

        return await _hardwareOrchestrator.GetTimersMaxLanes(track.Timers);
    }
    public async Task<IEnumerable<TimerDetails>> GetTrackTimers(Guid trackId)
    {

        Track? track = await _managementPersistence.GetTrack(trackId);
        if (track == null)
        {
            return Enumerable.Empty<TimerDetails>();
        }
        return await _hardwareOrchestrator.GetTimerDetails(track.Timers);
    }
}
