namespace NaeTime.Timing.Models;
public record Lap(Guid LapId, uint LapNumber, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
