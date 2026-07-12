namespace NaeTime.Events.Domain;
public record TrackRenamed(Guid TrackId, string? Name);
public record TrackDetectorAdded(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDetectorRemoved(Guid TrackId, Guid DetectorId);
public record TrackDetectorMoved(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDesigned(Guid TrackId, string Name, Guid[] DetectorIds);