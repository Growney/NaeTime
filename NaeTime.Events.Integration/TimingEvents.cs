using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Events.Integration;

public record LiveDetectionAssignedToSession(Guid SessionId, Guid DetectionId);
public record LiveDetectionOccurredWithNoActiveSession(Guid DetectionId);
public record LiveDetectionAssignedToPilot(Guid SessionId, Guid TrackId, Guid DetectionId, Guid PilotId);
public record PilotsMostRecentDetectionChanged(Guid SessionId, Guid TrackId, Guid DetectionId, Guid PilotId, DateTime UtcTime);
public record PilotsMostRecentDetectionCleared(Guid SessionId, Guid TrackId, Guid PilotId);
public record PilotsTimingChanged(Guid SessionId, Guid TrackId, Guid PilotId);