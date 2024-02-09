namespace NaeTime.Persistence.Models;
public record ActiveSplit(uint LapNumber, byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);