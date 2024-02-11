namespace NaeTime.Timing.Models;
public record Lap(uint LapNumber, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
