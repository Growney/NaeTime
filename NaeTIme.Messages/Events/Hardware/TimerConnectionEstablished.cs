namespace NaeTime.Messages.Events.Hardware;
public record TimerConnectionEstablished(Guid TimerId, long SoftwareTime, DateTime UtcTime);