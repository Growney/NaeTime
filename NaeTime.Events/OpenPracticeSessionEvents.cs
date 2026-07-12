namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId, TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);

public record OpenPracticeSessionTrackChanged(Guid SessionId,Guid OldTrackId, Guid NewTrackId);

public record OpenPracticeSessionMinimumLapTimeReset(Guid SessionId);
public record OpenPracticeSessionMinimumLapTimeSet(Guid SessionId, TimeSpan MinimumLapTime);
public record OpenPracticeSessionMaximumLapTimeReset(Guid SessionId);
public record OpenPracticeSessionMaximumLapTimeSet(Guid SessionId, TimeSpan MaximumLapTime);

public record OpenPracticeSessionCloned(Guid SessionId, Guid NewSessionId, string Name, Guid? TrackOverride);
public record OpenPracticeSessionCloneCreated(Guid SessionId, Guid NewSessionId, string Name);
