namespace NaeTime.Hardware.Messages;
public record TimerDetectionOccured(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
