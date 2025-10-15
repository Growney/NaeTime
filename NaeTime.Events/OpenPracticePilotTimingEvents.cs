namespace NaeTime.Events;
public record PilotOpenPracticeSessionTimingStarted(Guid PilotId, Guid SessionId);
public record OpenPracticeDetectionAddedToPilot(Guid PilotId, Guid SessionId, Guid DetectionId, byte OrdinalPosition, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);

public record OpenPracticeLapStarted(Guid PilotId, Guid SessionId, uint LapId, Guid StartDetectionId);
public record OpenPracticeLapCompleted(Guid PilotId, Guid SessionId, uint LapId, Guid StartDetectionId, Guid EndDetectionId, long DurationMilliseconds);
public record OpenPracticeLapEndSkipped(Guid PilotId, Guid SessionId, uint LapId);
public record OpenPracticeLapSplitStarted(Guid PilotId, Guid SessionId, uint LapId, byte SplitId, Guid StartDetectionId);
public record OpenPracticeLapSplitCompleted(Guid PilotId, Guid SessionId, uint LapId, byte SplitId, Guid StartDetectionId, Guid EndDetectionId, long DurationMilliseconds);
public record OpenPracticeLapSplitEndSkipped(Guid PilotId, Guid SessionId, uint LapId, byte SplitId);

