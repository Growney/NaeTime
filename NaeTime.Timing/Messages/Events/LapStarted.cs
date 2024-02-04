namespace NaeTime.Timing.Messages.Events;
public record LapStarted(Guid TrackId, byte Lane, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime);
