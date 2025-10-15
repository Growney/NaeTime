namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record HardwareDetectionAssignedToOpenPracticeSession(Guid DetectionId, Guid SessionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);