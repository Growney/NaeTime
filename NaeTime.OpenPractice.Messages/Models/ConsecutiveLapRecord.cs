namespace NaeTime.OpenPractice.Messages.Models;

public record ConsecutiveLapRecord(uint LapCap, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);