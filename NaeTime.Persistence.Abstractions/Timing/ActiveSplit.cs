namespace NaeTime.Persistence.Abstractions.Timing;
public record ActiveSplit(byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);
