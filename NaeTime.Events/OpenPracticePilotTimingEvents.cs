namespace NaeTime.Events;

public record OpenPracticePilotTimingStarted(Guid PilotId, Guid SessionId, Guid TrackId, Guid[] TrackDetectors, long? RedetectionDelayMilliseconds, long? MaximumLapMilliseconds);

public record OpenPracticePilotDetectionOccured(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggered(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte Lane, byte OrdinalPosition, byte TrackDetectorCount, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggeredToCloseToPreviousDetection(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime, Guid ConflictDetectionId, Guid? ConflictDetectionTimerId, byte ConclictDetectionLane, ulong? ConflictDetectionHardwareTime, long ConflictDetectionSoftwareTime, DateTime ConflictDetectionUtcTime);
public record OpenPracticePilotDetectionOccuredToCloseToPreviousDetection(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime, Guid ConflictDetectionId, Guid? ConflictDetectionTimerId, byte ConclictDetectionLane, ulong? ConflictDetectionHardwareTime, long ConflictDetectionSoftwareTime, DateTime ConflictDetectionUtcTime);
public record OpenPracticePilotDetectionOccuredOutOfOrder(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte TrackTimerOrdinal, byte TrackTimerTotal, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionTriggeredOutOfOrder(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, byte OrdinalPosition, byte TrackDetectorCount, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotDetectionOccuredOnInvalidTimer(Guid DetectionId, Guid SessionId, Guid TrackId, Guid PilotId, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);

public record OpenPracticeLapStarted(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime);
public record OpenPracticeLapCompleted(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime, Guid EndDetectionId, ulong? CompletedHardwareTime, long CompletedSoftwareTime, DateTime CompletedUtcTime, TimeSpan Duration);
public record OpenPracticeLapEndMissed(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, byte SkippedToTimerOrdinal, Guid SkippedToDetectionId, ulong? SkippedToHardwareTime, long SkippedToSoftwareTime, DateTime SkippedToUtcTime);
public record OpenPracticeLapSplitStarted(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, byte SplitIndex, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime);
public record OpenPracticeLapSplitCompleted(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, byte SplitIndex, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime, Guid EndDetectionId, ulong? CompletedHardwareTime, long CompletedSoftwareTime, DateTime CompletedUtcTime, TimeSpan Duration);
public record OpenPracticeLapSplitEndMissed(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, byte SplitIndex, byte SkippedToTimerOrdinal, Guid SkippedToDetectionId, ulong? SkippedToHardwareTime, long SkippedToSoftwareTime, DateTime SkippedToUtcTime);

public record OpenPracticeDetectionWithNoEffectOnRecord(Guid PilotId, Guid SessionId, Guid TrackId, Guid LapId, TimeSpan Duration);
public record OpenPracticeFastestLapRecordRecorded(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan Record, Guid LapId);
public record OpenPracticeConsecutiveLapRecordRecorded(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps);
public record OpenPracticeFastestConsecutiveLapsRecordRecorded(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan Record);
public record OpenPracticeFastestLapRecordImproved(Guid PilotId, Guid SessionId, Guid TrackId, TimeSpan OldRecord, TimeSpan NewRecord, Guid LapId);
public record OpenPracticeConsecutiveLapRecordImproved(Guid PilotId, Guid SessionId, Guid TrackId, uint OldConsecutiveLapCount, uint NewConsecutiveLapCount, Guid[] IncludedLaps);
public record OpenPracticeFastestConsecutiveLapsRecordImproved(Guid PilotId, Guid SessionId, Guid TrackId, uint LapCount, Guid[] IncludedLaps, TimeSpan OldRecord, TimeSpan NewRecord);

