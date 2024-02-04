namespace NaeTime.Messages.Models;
public record ActiveLap(uint LapNumber, long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);