namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);
public record DetectionAssignedToOpenPracticeSession(Guid DetectionId, Guid SessionId);
public record DetectionUnassignedOpenPracticeFromSession(Guid DetectionId, Guid SessionId);
