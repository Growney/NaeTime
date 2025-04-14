using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Management;
using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Orchestrator;

public class OpenPracticeOrchestrator : IOpenPracticeOrchestrator
{
    private readonly IOpenPracticeOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;
    private readonly IHardwareOrchestrator _hardware;
    private readonly IManagementOrchestrator _managementOrchestrator;

    public OpenPracticeOrchestrator(IOpenPracticeOrchestratorPersistence orchestratorPersistence, INaeTimeOrchestratorDistribution distribution, IHardwareOrchestrator hardware, IManagementOrchestrator managementOrchestrator)
    {
        _orchestratorPersistence = orchestratorPersistence;
        _distribution = distribution;
        _managementOrchestrator = managementOrchestrator;
        _hardware = hardware;
    }

    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId) => _orchestratorPersistence.ConfigureLanePilot(sessionId, lane, pilotId);

    public async Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz)
    {
        await _orchestratorPersistence.ConfigureLaneRadioFrequency(sessionId, lane, bandId, frequencyInMhz);
        ActiveSession? activeSession = await _managementOrchestrator.GetActiveSession();
        if (activeSession != null && activeSession.SessionId == sessionId)
        {
            await _hardware.ConfigureLaneRadioFrequency(lane, bandId, frequencyInMhz);
        }
    }

    public async Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled)
    {
        await _orchestratorPersistence.ConfigureLaneStatus(sessionId, lane, isEnabled);
        ActiveSession? activeSession = await _managementOrchestrator.GetActiveSession();
        if (activeSession != null && activeSession.SessionId == sessionId)
        {
            await _hardware.ConfigureLaneStatus(lane, isEnabled);
        }
    }

    public async Task<IEnumerable<OpenPracticeLaneConfiguration>> GetLaneConfigurations(Guid sessionId)
    {
        OpenPracticeSession? session = await _orchestratorPersistence.GetOpenPracticeSession(sessionId);

        if (session == null)
        {
            return Enumerable.Empty<OpenPracticeLaneConfiguration>();
        }

        IEnumerable<TimerDetails> timers = await _managementOrchestrator.GetTrackTimers(session.TrackId);
        IEnumerable<Guid> timerIds = timers.Select(x => x.Id);
        IEnumerable<LaneConfiguration> laneConfigurations = await _hardware.GetLaneConfigurations(timerIds);
        IEnumerable<PilotLaneConfiguration> pilotLaneConfigurations = await _orchestratorPersistence.GetLaneConfigurations(sessionId);

        Dictionary<byte, PilotLaneConfiguration> indexedPilotLaneConfigurations = pilotLaneConfigurations.ToDictionary(x => x.Lane);

        List<OpenPracticeLaneConfiguration> lanes = new();

        foreach (LaneConfiguration config in laneConfigurations)
        {
            indexedPilotLaneConfigurations.TryGetValue(config.Lane, out PilotLaneConfiguration? pilotLaneConfiguration);
            lanes.Add(new OpenPracticeLaneConfiguration(config.Lane,
                pilotLaneConfiguration?.PilotId,
                config));
        }

        return lanes;
    }

    public Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionAverageLapLeaderboardPositions(sessionId);

    public Task<AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionAverageLapRecord(sessionId, pilotId);

    public Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap) =>
        _orchestratorPersistence.GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(sessionId, lapCap);

    public Task<IEnumerable<ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionConsecutiveLapRecords(sessionId, pilotId);

    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionLaps(sessionId, pilotId);

    public Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetOpenPracticeSessionLapPilotLapRecords(sessionId, pilotId);

    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionLaps(sessionId);

    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId) =>
        _orchestratorPersistence.GetOpenPracticeSessionLap(lapId);

    public Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionSingleLapLeaderboardPositions(sessionId);

    public Task<SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionSingleLapRecord(sessionId, pilotId);

    public Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionTotalLapLeaderboardPositions(sessionId);

    public Task<TotalLapRecord?> GetPilotOpenPracticeSessionTotalLapRecord(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionTotalLapRecord(sessionId, pilotId);

    public Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions() =>
        _orchestratorPersistence.GetOpenPracticeSessions();

    public Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSession(sessionId);

    public Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionTrackedConsecutiveLaps(sessionId);

    public Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds) =>
        _orchestratorPersistence.GetOpenPracticeLaps(lapIds);
}
