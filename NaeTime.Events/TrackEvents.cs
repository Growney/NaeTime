namespace NaeTime.Events;
public record TrackRenamed(Guid TrackId, string? Name);
public record TrackDetectorAdded(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDetectorRemoved(Guid TrackId, Guid DetectorId);
public record TrackDetectorMoved(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDesigned(Guid TrackId, string Name, Guid[] DetectorIds);

public record TrackMaximumLapTimeReset(Guid TrackId);
public record TrackRedetectionDelayReset(Guid TrackId);
public record TrackMaximumLapTimeConfigured(Guid TrackId, long MaximumMilliseconds);
public record TrackRedetectionDelayConfigured(Guid TrackId, long DelayMilliseconds);