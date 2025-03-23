namespace NaeTime.Persistence.Abstractions.OpenPractice;

public interface IOpenPracticeRepository
{
    public Task<IEnumerable<AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId);
    public Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint LapCap);
    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<LapRecord>> GetOpenPracticeSessionLapPilotLapRecords(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId);
    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId);
    public Task<IEnumerable<SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId);
    public Task<IEnumerable<TotalLapLeaderboardPosition>> GetOpenPracticeSessionTotalLapLeaderboardPositions(Guid sessionId);
    public Task<IEnumerable<OpenPracticeSession>> GetOpenPracticeSessions();
    public Task<OpenPracticeSession?> GetOpenPracticeSession(Guid sessionId);

}
