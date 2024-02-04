namespace NaeTime.Messages.Models;
public record ActiveSplit(uint LapNumber, byte SplitNumber, long StartedSoftwareTime, DateTime StartedUtcTime);
