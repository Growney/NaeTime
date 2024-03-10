namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeSessionDetectionTriggered(Guid SessionId, byte Lane, Guid TimerId);
