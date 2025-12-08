namespace NaeTime.Events;
public record OpenPracticeSessionActivated(Guid SessionId);
public record OpenPracticeSessionDeactivated(Guid SessionId);
public record OpenPracticeSessionScheduled(Guid SessionId, string Name, Guid TrackId, Guid[] TrackDetectors, TimeSpan? MinimumLapTime, TimeSpan? MaximumLapTime);
public record OpenPracticeSessionRenamed(Guid Sessionid, string Name);

public record OpenPracticeSessionMinimumLapTimeReset(Guid SessionId);
public record OpenPracticeSessionMinimumLapTimeSet(Guid SessionId, TimeSpan MinimumLapTime);
public record OpenPracticeSessionMaximumLapTimeReset(Guid SessionId);
public record OpenPracticeSessionMaximumLapTimeSet(Guid SessionId, TimeSpan MaximumLapTime);
public record OpenPracticeSessionPilotMinimumLapTimeReset(Guid SessionId, Guid PilotId);
public record OpenPracticeSessionPilotMinimumLapTimeSet(Guid SessionId, Guid PilotId, TimeSpan MinimumLapTime);
public record OpenPracticeSessionPilotMaximumLapTimeReset(Guid SessionId, Guid PilotId);
public record OpenPracticeSessionPilotMaximumLapTimeSet(Guid SessionId, Guid PilotId, TimeSpan MaximumLapTime);
