namespace NaeTime.Timing.Messages.Events;
public record SessionDetectionTriggered(Guid SessionId, byte Lane, byte Split);
