namespace NaeTime.Timing.Messages.Events;
public record SessionInvalidationTriggered(Guid SessionId, byte Lane);