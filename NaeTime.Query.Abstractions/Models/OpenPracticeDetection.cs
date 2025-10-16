namespace NaeTime.Query.Abstractions.Models;
public record OpenPracticeDetection(Guid Id, Guid SessionId, ulong? HardwareTime, long SoftwareTime, DateTime UtcTime);