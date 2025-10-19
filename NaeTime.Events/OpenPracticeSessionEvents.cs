namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId, Guid[] TrackDetectors);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);