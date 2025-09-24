namespace NaeTime.Query.Abstractions.Models;
public record Track(Guid Id, string? Name, Detector[] Detectors, byte MaxLanes, long? MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds);
