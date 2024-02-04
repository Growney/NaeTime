namespace NaeTime.Messages.Events.Hardware;
public record TimerDisconnected(Guid TimerId, long SoftwareTime, DateTime UtcTime);
