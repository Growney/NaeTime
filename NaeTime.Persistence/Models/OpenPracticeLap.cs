namespace NaeTime.Persistence.Models;

public record OpenPracticeLap(Guid LapId, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, OpenPracticeLapStatus Status, long TotalMilliseconds);
