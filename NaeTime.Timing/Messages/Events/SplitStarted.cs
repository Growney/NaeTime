namespace NaeTime.Timing.Messages.Events;
public record SplitStarted(Guid TrackId, byte Lane, uint LapNumber, byte Split, long SoftwareTime, DateTime UtcTime);
