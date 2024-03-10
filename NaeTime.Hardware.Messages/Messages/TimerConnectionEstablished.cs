namespace NaeTime.Hardware.Messages.Messages;
public record TimerConnectionEstablished(Guid TimerId, long SoftwareTime, DateTime UtcTime);