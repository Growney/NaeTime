namespace NaeTime.Hardware.Messages;
public record TimerDisconnected(Guid TimerId, long SoftwareTime, DateTime UtcTime);
