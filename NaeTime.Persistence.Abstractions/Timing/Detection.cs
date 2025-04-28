namespace NaeTime.Persistence.Abstractions.Timing;
public record Detection(Guid Id, Guid SessionId, Guid TrackId, Guid TimerId, int TimerIndex, byte Lane, Guid? PilotId, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime) : IDetection;


