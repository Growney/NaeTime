namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);
public record DetectionAddedToOpenPracticeSession(Guid DetectionId, Guid SessionId, byte OrdinalPosition, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionRemovedFromOpenPracticeSession(Guid DetectionId, Guid SessionId);
public record OpenPracticeDetectionAssignedToPilot(Guid DetectionId, Guid SessionId, Guid PilotId, byte OrdinalPosition, byte TrackDetectorCount, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeDetectionUnassignedFromPilot(Guid DetectionId, Guid PilotId);