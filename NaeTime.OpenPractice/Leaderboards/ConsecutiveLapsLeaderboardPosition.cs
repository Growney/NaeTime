namespace NaeTime.OpenPractice.Leaderboards;
public record ConsecutiveLapsLeaderboardPosition(int Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletion, IEnumerable<Guid> IncludedLaps);
