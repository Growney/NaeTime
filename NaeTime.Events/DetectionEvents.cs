namespace NaeTime.Events;
public record HardwareDetectionOccured(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
public record HardwareDetectionOccuredWithNoActiveSession(Guid Id, Guid TimerId, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);
