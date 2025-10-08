namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionTriggered(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime);
public record DetectionBoundToOpenPracticeSession(Guid DetectionId, Guid SessionId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionUnboundFromOpenPracticeSession(Guid DetectionId, Guid SessionId);