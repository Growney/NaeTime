using NaeTime.Messages.Models;

namespace NaeTime.Timing.Messages.Events;
public record LapInvalidated(Guid TrackId, byte Lane, uint LapNumber, long SoftwareTime, DateTime UtcTime, ulong? HardwareTime, long TotalTime, LapInvalidReason Reason);