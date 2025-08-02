namespace NaeTime.Events;
public record OpenPracticeSessionScheduled(Guid SessionId, Guid TrackId);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
