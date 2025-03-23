namespace NaeTime.Persistence.Abstractions.Timing;
public record ActiveLap(long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
