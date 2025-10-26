namespace NaeTime.Events;
public record OpenPracticeSessionLanePilotSet(Guid SessionId, byte Lane, Guid PilotId);
public record OpenPracticeSessionLanePilotReset(Guid SessionId, byte Lane);
public record OpenPracticeSessionLaneVideoFrequencyTuned(Guid SessionId, byte Lane, byte? BandId, int FrequencyInMHz, Guid[] TimerIds);
public record OpenPracticeSessionLaneEnabled(Guid SessionId, byte Lane, Guid[] TimerIds);
public record OpenPracticeSessionLaneDisabled(Guid SessionId, byte Lane, Guid[] TimerIds);