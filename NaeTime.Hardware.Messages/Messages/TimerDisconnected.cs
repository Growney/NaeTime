namespace NaeTime.Hardware.Messages.Messages;
public record TimerDisconnected(Guid TimerId, long SoftwareTime, DateTime UtcTime);
