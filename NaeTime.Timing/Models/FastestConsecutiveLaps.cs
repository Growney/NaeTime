namespace NaeTime.Timing.Models;
public record FastestConsecutiveLaps(uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);