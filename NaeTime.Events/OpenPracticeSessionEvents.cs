namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);
public record DetectionAssignedToOpenPracticeSession(Guid DetectionId, Guid SessionId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record DetectionUnassignedFromOpenPracticeSession(Guid DetectionId, Guid SessionId);
public record OpenPracticeDetectionAssignedToPilot(Guid DetectionId, Guid PilotId);
public record OpenPracticeDetectionUnassignedFromPilot(Guid DetectionId, Guid PilotId);