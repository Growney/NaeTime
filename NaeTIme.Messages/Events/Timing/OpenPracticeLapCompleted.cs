namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeLapCompleted(Guid LapId, Guid SessionId, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, long TotalMilliseconds);