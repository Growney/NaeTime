using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Management;

namespace NaeTime.Orchestrator;

public class OpenPracticeOrchestrator : IOpenPracticeOrchestrator
{
    private readonly IOpenPracticeOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;
    private readonly IHardwareOrchestrator _hardware;
    private readonly INaeTimePersistence _persistence;

    public OpenPracticeOrchestrator(IOpenPracticeOrchestratorPersistence orchestratorPersistence, INaeTimeOrchestratorDistribution distribution, IHardwareOrchestrator hardware, INaeTimePersistence persistence)
    {
        _orchestratorPersistence = orchestratorPersistence;
        _distribution = distribution;
        _persistence = persistence;
        _hardware = hardware;
    }

    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId) => _orchestratorPersistence.ConfigureLanePilot(sessionId, lane, pilotId);
    public async Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        await _orchestratorPersistence.ConfigureLaneRadioFrequency(sessionId, lane, bandId, frequencyInMhz);
        ActiveSession? activeSession = await _persistence.Management.GetActiveSession();
        if (activeSession != null && activeSession.SessionId == sessionId)
        {
            await _hardware.ConfigureLaneRadioFrequency(lane, bandId, frequencyInMhz);
        }
    }
    public async Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled)
    {
        await _orchestratorPersistence.ConfigureLaneStatus(sessionId, lane, isEnabled);
        ActiveSession? activeSession = await _persistence.Management.GetActiveSession();
        if (activeSession != null && activeSession.SessionId == sessionId)
        {
            await _hardware.ConfigureLaneStatus(lane, isEnabled);
        }
    }
}
