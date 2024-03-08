namespace NaeTime.Persistence.Models;
public record ConsecutiveLapLeaderboardPosition(Guid SessionId, Guid PilotId, uint LapCap, int Position, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
