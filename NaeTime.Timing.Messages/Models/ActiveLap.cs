namespace NaeTime.Timing.Messages.Models;
public record ActiveLap(long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
