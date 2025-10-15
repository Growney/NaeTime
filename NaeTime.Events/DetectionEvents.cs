namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionOccured(Guid DetectionId, Guid SessionId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnDisabledLane(Guid DetectionId, Guid SessionId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(Guid DetectionId, Guid SessionId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionOccured(Guid DetectionId, Guid SessionId, Guid PilotId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggered(Guid DetectionId, Guid SessionId, Guid PilotId, byte OrdinalPosition, byte TrackDetectorCount, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
