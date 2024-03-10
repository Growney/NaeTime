namespace NaeTime.Timing.Messages.Events;
public record SplitCompleted(Guid SessionId, byte Lane, uint LapNumber, byte Split, long StartedSoftwareTime, DateTime StartedUtcTime, long TotalTime);
