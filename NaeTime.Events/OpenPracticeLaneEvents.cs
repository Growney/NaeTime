namespace NaeTime.Events;

public record OpenPracticeSessionLaneConfigured(Guid SessionId, byte Lane, byte? BandId, int FrequencyInMHz);
public record OpenPracticeSessionLanePilotSet(Guid SessionId, byte Lane, Guid PilotId);
public record OpenPracticeSessionLanePilotReset(Guid SessionId, byte Lane);
public record OpenPracticeSessionLaneVideoFrequencyTuned(Guid SessionId, byte Lane, byte? BandId, int FrequencyInMHz);
public record OpenPracticeSessionLaneStatusSet(Guid SessionId, byte Lane, bool IsEnabled);

public record OpenPracticeSessionLaneCloned(Guid SessionId, byte Lane);
public record OpenPracticeSessionLaneCloneCreated(Guid SessionId, Guid OldSessionId, byte Lane);