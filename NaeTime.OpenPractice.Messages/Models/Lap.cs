namespace NaeTime.OpenPractice.Messages.Models;
public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);