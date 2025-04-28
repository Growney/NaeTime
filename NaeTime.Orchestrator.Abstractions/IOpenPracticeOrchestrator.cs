using NaeTime.Persistence.Abstractions.OpenPractice;

namespace NaeTime.Orchestrator.Abstractions;

public interface IOpenPracticeOrchestrator
{
    public Task ConfigureLaneStatus(Guid sessionId, byte lane, bool isEnabled);
    public Task ConfigureLaneRadioFrequency(Guid sessionId, byte lane, byte? bandId, int frequencyInMhz);
    public Task ConfigureLanePilot(Guid sessionId, byte lane, Guid pilotId);


    public Task<IEnumerable<OpenPracticeLaneConfiguration>> GetLaneConfigurations(Guid sessionId);
    public Task<OpenPracticeLaneConfiguration> GetLaneConfiguration(Guid sessionId, byte lane);
    public Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId);
    public Task<AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap);
    public Task<IEnumerable<ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId);
    public Task<SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId);
    public Task<TotalLapRecord?> GetPilotOpenPracticeSessionTotalLapRecord(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions();
    public Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId);
    public Task<IEnumerable<uint>> GetOpenPracticeSessionTrackedConsecutiveLaps(Guid sessionId);
}
