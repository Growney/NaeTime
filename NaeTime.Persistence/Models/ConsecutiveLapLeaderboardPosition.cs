namespace NaeTime.Persistence.Models;
public record ConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);

