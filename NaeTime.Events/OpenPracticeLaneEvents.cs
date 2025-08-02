namespace NaeTime.Events;
public record OpenPracticeSessionLanePilotSet(Guid SessionId, byte Lane, Guid PilotId);
public record OpenPracticeSessionLanePilotReset(Guid SessionId, byte Lane);
public record OpenPracticeSessionLaneVideoFrequencyTuned(Guid SessionId, byte Lane, byte? BandId, int FrequencyInMHz);
public record OpenPracticeSessionLaneEnabled(Guid SessionId, byte Lane);
public record OpenPracticeSessionLaneDisabled(Guid SessionId, byte Lane);