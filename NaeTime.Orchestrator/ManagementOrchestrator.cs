using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Management;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Orchestrator;

public class ManagementOrchestrator : IManagementOrchestrator
{
    private readonly IManagementOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimePersistence _persistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public ManagementOrchestrator(IManagementOrchestratorPersistence orchestratorPersistence, INaeTimePersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _orchestratorPersistence = orchestratorPersistence ?? throw new ArgumentNullException(nameof(orchestratorPersistence));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
    }

    public async Task ActivateOpenPracticeSession(Guid sessionId)
    {
        ActiveSession? currentActiveSession = await _persistence.Management.GetActiveSession();

        if (currentActiveSession != null)
        {
            await _distribution.Distribute(new SessionDeactivated(currentActiveSession.SessionId));
        }

        await _orchestratorPersistence.ActivateOpenPracticeSession(sessionId);
        await _distribution.Distribute(new OpenPracticeSessionActivated(sessionId));
    }
    public Task<Guid> CreatePilot(string? firstname, string? lastname, string? callsign) => _orchestratorPersistence.CreatePilot(firstname, lastname, callsign);
    public Task<Guid> CreateTrack(string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers) => _orchestratorPersistence.CreateTrack(name, MinimumLapMilliseconds, MaximumLapMilliseconds, timers);
    public Task<bool> UpdatePilot(Guid pilotId, string? firstname, string? lastname, string? callsign) => _orchestratorPersistence.UpdatePilot(pilotId, firstname, lastname, callsign);
    public Task<bool> UpdateTrack(Guid id, string? name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> timers) => _orchestratorPersistence.UpdateTrack(id, name, MinimumLapMilliseconds, MaximumLapMilliseconds, timers);
}
