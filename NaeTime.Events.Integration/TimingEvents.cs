using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Events.Integration;

public record LiveDetectionAssignedToSession(Guid SessionId, Guid DetectionId);
public record LiveDetectionOccurredWithNoActiveSession(Guid DetectionId);
public record LiveDetectionAssignedToPilot(Guid SessionId, Guid DetectionId, Guid PilotId);