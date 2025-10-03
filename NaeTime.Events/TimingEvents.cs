namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid DetectionId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionTriggered(Guid DetectionId, byte Lane, long SoftwareTime, DateTime UtcTime);
