namespace NaeTime.Events;

public record OpenPracticePilotTimingStarted(Guid SessionId, Guid PilotId);

public record OpenPracticeSessionPilotMinimumLapTimeReset(Guid SessionId, Guid PilotId);
public record OpenPracticeSessionPilotMinimumLapTimeSet(Guid SessionId, Guid PilotId, TimeSpan MinimumLapTime);
public record OpenPracticeSessionPilotMaximumLapTimeReset(Guid SessionId, Guid PilotId);
public record OpenPracticeSessionPilotMaximumLapTimeSet(Guid SessionId, Guid PilotId, TimeSpan MaximumLapTime);

public record OpenPracticePilotPackEndAdded(Guid SessionId, Guid PilotId, Guid PackEndId, long SoftwareTime, DateTime UtcTime);
public record OpenPracticePilotPackEndRemoved(Guid SessionId, Guid PilotId, Guid PackEndId);
