namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeSplitCompleted(Guid SessionId, uint LapNumber, byte Split, byte Lane, Guid? PilotId, long SoftwareTime, DateTime UtcTime, long TotalTime);
