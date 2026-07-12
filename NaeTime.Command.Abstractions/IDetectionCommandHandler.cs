using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command.Abstractions;

public interface IDetectionCommandHandler
{
    Task Trigger(Guid id, Guid timerId, byte laneId, ulong hardwareTimer, long softwareTimer, DateTime utcTime);
    Task Trigger(Guid id, Guid timerId, byte laneId);
    Task OverrideSession(Guid detectionId, Guid sessionId);
    Task OverridePilot(Guid detectionId, Guid pilotId);
    Task SetStatus(Guid detectionId, bool isValid);
    Task Move(Guid detectionId, long softwareTime, DateTime utcTime);
}
