namespace NaeTime.Messages.Events.Timing;
public record SessionInvalidationTriggered(Guid SessionId, byte Lane);