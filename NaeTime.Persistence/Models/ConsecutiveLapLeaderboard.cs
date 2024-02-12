namespace NaeTime.Persistence.Models;

public record ConsecutiveLapLeaderboard(Guid LeaderboardId, uint ConsecutiveLaps, IEnumerable<ConsecutiveLapLeaderboardPosition> Positions);

