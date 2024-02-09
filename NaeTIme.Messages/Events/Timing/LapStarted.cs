namespace NaeTime.Messages.Events.Timing;
public record LapStarted(Guid SessionId, byte Lane, uint LapNumber, long StartedSoftwareTime, DateTime StartedUtcTime, ulong? StartedHardwareTime);
