namespace NaeTime.Messages.Events.Timing;
public record SplitSkipped(Guid SessionId, byte Lane, uint LapNumber, byte Split);
