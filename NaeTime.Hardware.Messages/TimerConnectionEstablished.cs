namespace NaeTime.Hardware.Messages;
public record TimerConnectionEstablished(Guid TimerId, long SoftwareTime, DateTime UtcTime);