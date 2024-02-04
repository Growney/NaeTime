namespace NaeTime.Messages.Events.Timing;
public record TimerDetectionOccured(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
