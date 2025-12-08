namespace NaeTime.Events;

public record OpenPracticeHardwareDetectionOccured(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnDisabledLane(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(Guid DetectionId, Guid SessionId, Guid TimerId, Guid TrackId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticeDetectionTriggerIgnoredOnUnassignedLane(Guid DetectionId, Guid SessionId, Guid TrackId, byte OrdinalPosition, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);

public record OpenPracticePilotDetectionOccured(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggered(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte Lane, byte OrdinalPosition, byte TrackDetectorCount, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggeredToCloseToPreviousDetection(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime, Guid ConflictDetectionId, Guid? ConflictDetectionTimerId, byte ConclictDetectionLane, ulong? ConflictDetectionHardwareTime, long ConflictDetectionSoftwareTime, DateTime ConflictDetectionUtcTime);
public record OpenPracticePilotDetectionOccuredToCloseToPreviousDetection(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime, Guid ConflictDetectionId, Guid? ConflictDetectionTimerId, byte ConclictDetectionLane, ulong? ConflictDetectionHardwareTime, long ConflictDetectionSoftwareTime, DateTime ConflictDetectionUtcTime);
public record OpenPracticePilotDetectionOccuredOutOfOrder(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggeredOutOfOrder(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte OrdinalPosition, byte TrackDetectorCount, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionOccuredOnInvalidTimer(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);

public record OpenPracticePilotDetectionInvalidated(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId);
public record OpenPracticePilotDetectionValidated(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId);
public record OpenPracticePilotPackEndInsertedAfterDetection(Guid DetectionId,Guid PackEndId, Guid SessionId, Guid TrackId, Guid PilotId);
public record OpenPracticePilotPackEndInsertedBeforeDetection(Guid DetectionId,Guid PackEndId, Guid SessionId, Guid TrackId, Guid PilotId);
public record OpenPracticePilotPackEndRemoved(Guid PackEndId,Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId);
public record OpenPracticePilotTimingChangeOccured(Guid SessionId, Guid TrackId, Guid PilotId);

public record OpenPracticePilotLastDetectionRevised(Guid SessionId, Guid TrackId, Guid PilotId, Guid DetectionId, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDowned(Guid SessionId, Guid TrackId, Guid PilotId);