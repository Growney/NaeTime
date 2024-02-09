namespace NaeTime.Messages.Events.Timing;
public record SplitStarted(Guid SessionId, byte Lane, uint LapNumber, byte Split, long StartedSoftwareTime, DateTime StartedUtcTime);
