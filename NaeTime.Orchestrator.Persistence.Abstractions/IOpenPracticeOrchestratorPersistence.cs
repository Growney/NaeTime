using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface IOpenPracticeOrchestratorPersistence
{
    public Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled);
    public Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId);

    public Task<IEnumerable<PilotLaneConfiguration>> GetLaneConfigurations(Guid sessionId);
    public Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId);
    public Task<AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap);
    public Task<IEnumerable<ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId);
    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId);
    public Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId);
    public Task<SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId);
    public Task<TotalLapRecord?> GetPilotOpenPracticeSessionTotalLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions();
    public Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId);
    public Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId);
    public Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds);
}
