namespace NaeTime.Persistence.Abstractions.OpenPractice;

public record ConsecutiveLapRecord(uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);