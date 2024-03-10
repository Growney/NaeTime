namespace NaeTime.Timing.Messages.Events;
public record SplitSkipped(Guid SessionId, byte Lane, uint LapNumber, byte Split);
