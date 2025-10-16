namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record HardwareDetectionOccuredWithNoActiveSession(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionOccured(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnDisabledLane(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionAssignedToPilot(Guid DetectionId, Guid SessionId, Guid PilotId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);