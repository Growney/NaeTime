namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionTriggered(byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionAssignedToSession(Guid SessionId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
