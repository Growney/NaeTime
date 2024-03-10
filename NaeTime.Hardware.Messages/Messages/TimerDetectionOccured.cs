namespace NaeTime.Hardware.Messages.Messages;
public record TimerDetectionOccured(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
