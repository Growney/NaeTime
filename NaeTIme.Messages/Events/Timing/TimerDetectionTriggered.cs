namespace NaeTime.Messages.Events.Timing;
public record TimerDetectionTriggered(Guid TimerId, byte Lane, long SoftwareTime, DateTime UtcTime);