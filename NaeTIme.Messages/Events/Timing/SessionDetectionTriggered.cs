namespace NaeTime.Messages.Events.Timing;
public record SessionDetectionTriggered(Guid SessionId, byte Lane, byte Split);
