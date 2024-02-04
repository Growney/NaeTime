namespace NaeTime.Timing.Messages.Events;
public record SplitSkipped(Guid TrackId, byte Lane, uint LapNumber, byte Split);
