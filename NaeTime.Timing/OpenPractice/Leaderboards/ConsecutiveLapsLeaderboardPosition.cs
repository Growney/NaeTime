namespace NaeTime.Timing.OpenPractice.Leaderboards;
public record ConsecutiveLapsLeaderboardPosition(uint Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletion, IEnumerable<Guid> IncludedLaps);
