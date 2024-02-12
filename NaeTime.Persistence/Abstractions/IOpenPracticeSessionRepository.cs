using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface IOpenPracticeSessionRepository
{
    public Task<OpenPracticeSession?> Get(Guid sessionId);
    public Task AddOrUpdate(Guid sessionId, string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds);
    public Task AddLapToSession(Guid sessionId, Guid lapId, Guid pilotId, uint lapNumber, OpenPracticeLapStatus status, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds);
    public Task SetSessionLanePilot(Guid sessionId, byte lane, Guid pilotId);
    public Task UpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId, IEnumerable<SingleLapLeaderboardPosition> positions);
    public Task UpdateConsecutiveLapsLeaderboardPositions(Guid sessionId, Guid leaderboardId, IEnumerable<ConsecutiveLapLeaderboardPosition> positions);
    public Task AddOrUpdateConsecutiveLapsLeaderboard(Guid sessionId, Guid leaderboardId, uint consecutiveLaps);
    public Task AddOrUpdateSingleLapLeaderboard(Guid sessionId, Guid leaderboardId);
    public Task RemoveLeaderboard(Guid sessionId, Guid leaderboardId);
}
