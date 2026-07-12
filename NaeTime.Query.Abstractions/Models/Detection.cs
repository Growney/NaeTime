namespace NaeTime.Query.Abstractions.Models;

public record Detection(Guid Id, Guid? SessionId, Guid? TrackId, Guid? PilotId, Guid? TimerId, bool IsValid, byte Lane, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime) : ITimingOccasion;