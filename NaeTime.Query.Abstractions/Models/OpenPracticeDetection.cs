namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeDetection(Guid Id, Guid SessionId, byte OrdinalPosition, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);