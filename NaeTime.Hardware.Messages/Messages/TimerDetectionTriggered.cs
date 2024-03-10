namespace NaeTime.Hardware.Messages.Messages;
public record TimerDetectionTriggered(Guid TimerId, byte Lane, long SoftwareTime, DateTime UtcTime);