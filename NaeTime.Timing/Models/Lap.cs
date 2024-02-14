namespace NaeTime.Timing.Models;
public record Lap(Guid LapId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
