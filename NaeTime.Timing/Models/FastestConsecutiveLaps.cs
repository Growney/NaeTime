namespace NaeTime.Timing.Models;
public record FastestConsecutiveLaps(uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc);