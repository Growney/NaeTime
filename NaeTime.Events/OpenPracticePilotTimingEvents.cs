namespace NaeTime.Events;

public record OpenPracticeLapStarted(Guid PilotId, Guid SessionId, Guid LapId, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime);
public record OpenPracticeLapCompleted(Guid PilotId, Guid SessionId, Guid LapId, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime, Guid EndDetectionId, ulong? CompletedHardwareTime, long CompletedSoftwareTime, DateTime CompletedUtcTime);
public record OpenPracticeLapEndMissed(Guid PilotId, Guid SessionId, Guid LapId, byte SkippedToTimerOrdinal, Guid SkippedToDetectionId, ulong? SkippedToHardwareTime, long SkippedToSoftwareTime, DateTime SkippedToUtcTime);
public record OpenPracticeLapSplitStarted(Guid PilotId, Guid SessionId, Guid LapId, byte SplitIndex, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime);
public record OpenPracticeLapSplitCompleted(Guid PilotId, Guid SessionId, Guid LapId, byte SplitIndex, Guid StartDetectionId, ulong? StartedHardwareTime, long StartedSoftwareTime, DateTime StartedUtcTime, Guid EndDetectionId, ulong? CompletedHardwareTime, long CompletedSoftwareTime, DateTime CompletedUtcTime);
public record OpenPracticeLapSplitEndSkipped(Guid PilotId, Guid SessionId, Guid LapId, byte SplitIndex, byte SkippedToTimerOrdinal, Guid SkippedToDetectionId, ulong? SkippedToHardwareTime, long SkippedToSoftwareTime, DateTime SkippedToUtcTime);

