namespace NaeTime.Persistence.Models;
public record ActiveLap(long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);