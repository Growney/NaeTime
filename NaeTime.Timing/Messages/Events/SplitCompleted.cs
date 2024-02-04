namespace NaeTime.Timing.Messages.Events;
public record SplitCompleted(Guid TrackId, byte Lane, uint LapNumber, byte Split, long SoftwareTime, DateTime UtcTime, long TotalTime);
