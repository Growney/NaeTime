namespace NaeTime.Timing.Messages.Events;
public record LapCompleted(Guid TrackId, byte Lane, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime, long TotalTime);