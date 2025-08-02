namespace NaeTime.Events;
public record TrackRenamed(Guid TrackId, string? Name);
public record TrackDetectorAdded(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDetectorRemoved(Guid TrackId, Guid DetectorId);
public record TrackDetectorMoved(Guid TrackId, Guid DetectorId, byte OrdinalPosition);
public record TrackDesigned(Guid TrackId, string Name, Guid[] DetectorIds);

public record TrackMaximumLapTimeConfigured(Guid SessionId, long MaximumMilliseconds);
public record TrackMinimumLapTimeConfigured(Guid SessionId, long MinimumMilliseconds);
public record TrackPilotMaximumLapTimeReset(Guid SessionId, Guid PilotId);
public record TrackPilotMinimumLapTimeReset(Guid SessionId, Guid PilotId);
public record TrackPilotMaximumLapTimeConfigured(Guid SessionId, Guid PilotId, long MaximumMilliseconds);
public record TrackPilotMinimumLapTimeConfigured(Guid SessionId, Guid PilotId, long MinimumMilliseconds);