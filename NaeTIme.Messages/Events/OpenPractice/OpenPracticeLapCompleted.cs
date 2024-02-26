namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeLapCompleted(Guid LapId, Guid SessionId, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, long TotalMilliseconds);