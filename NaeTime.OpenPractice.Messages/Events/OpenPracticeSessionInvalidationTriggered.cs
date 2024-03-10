namespace NaeTime.Timing.Messages.Events;
public record OpenPracticeSessionInvalidationTriggered(Guid SessionId, byte Lane);