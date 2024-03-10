namespace NaeTime.Timing.Messages.Events;
public record SessionDetectionOccured(Guid SessionId, Guid TimerId, byte Lane, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
