namespace NaeTime.Messages.Events.Timing;
public record SplitCompleted(Guid SessionId, byte Lane, uint LapNumber, byte Split, long StartedSoftwareTime, DateTime StartedUtcTime, long TotalTime);
