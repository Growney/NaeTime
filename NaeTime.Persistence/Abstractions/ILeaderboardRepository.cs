using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface ILeaderboardRepository
{
    Task RemoveConsecutiveLapLeaderboardPosition(Guid sessionId, Guid pilotId, uint lapCap);
    Task AddOrUpdateConsecutiveLapLeaderboardPosition(Guid sessionId, Guid pilotId, uint lapCap, int position, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps);

    Task<IEnumerable<ConsecutiveLapLeaderboardPosition>> GetConsecutiveLapLeaderboardPositions(Guid sessionId, uint lapCap);
}
