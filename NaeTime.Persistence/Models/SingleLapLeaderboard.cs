namespace NaeTime.Persistence.Models;

public record SingleLapLeaderboard(Guid LeaderboardId, IEnumerable<SingleLapLeaderboardPosition> Positions);

