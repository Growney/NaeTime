namespace NaeTime.Persistence.Models;

public record OpenPracticeLap(Guid LapId, Guid PilotId, uint LapNumber, DateTime StartedUtc, DateTime FinishedUtc, OpenPracticeLapStatus Status, long TotalMilliseconds);
