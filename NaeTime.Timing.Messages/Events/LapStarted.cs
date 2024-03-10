namespace NaeTime.Timing.Messages.Events;
public record LapStarted(Guid SessionId, byte Lane, uint LapNumber, long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
