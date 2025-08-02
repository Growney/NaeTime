namespace NaeTime.Query.Abstractions.Models;
public record Detector(Guid Id, string? Name, DetectorType Type, byte SupportedLanes);
