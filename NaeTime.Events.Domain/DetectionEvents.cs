namespace NaeTime.Events.Domain;
public record HardwareDetectionOccured(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionManuallyTriggered(Guid Id, Guid TimerId, byte Lane, long SoftwareTime, DateTime UtcTime);

public record DetectionSessionOverridden(Guid Id, Guid SessionId);
public record DetectionPilotOverridden(Guid Id, Guid PilotId);
public record DetectionStatusSet(Guid Id, bool IsValid);
public record DetectionMoved(Guid Id, long SoftwareTime, DateTime UtcTime);