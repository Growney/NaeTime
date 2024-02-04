namespace NaeTime.Messages.Events.Timing.Practice;
public record PracticeSplitStarted(Guid SessionId, uint LapNumber, byte Split, byte Lane, Guid? PilotId, long SoftwareTime, DateTime UtcTime);
