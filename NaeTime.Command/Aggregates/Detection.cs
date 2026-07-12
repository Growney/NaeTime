using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Command.Aggregates;

public class Detection : AggregateRoot<Guid>
{
    private Guid? _overriddenSessionId;
    private Guid? _overriddenPilotId;
    private bool _isValid;
    private bool _canBeMoved = false;
    public Detection()
    {

    }
    public Detection(Guid id, Guid timerId, byte laneId, ulong hardwareTimer, long softwareTimer, DateTime utcTime)
    {
        Raise(new HardwareDetectionOccured(id, timerId, laneId, hardwareTimer, softwareTimer, utcTime));
    }
    public Detection(Guid id, Guid timerId, byte laneId, long softwareTimer, DateTime utcTime)
    {
        Raise(new DetectionManuallyTriggered(id, timerId, laneId, softwareTimer, utcTime));
    }

    private void When(HardwareDetectionOccured detection)
    {
        Id = detection.Id;
    }
    private void When(DetectionManuallyTriggered detection)
    {
        Id = detection.Id;
        _canBeMoved = true;
    }

    public void OverrideSession(Guid sessionId)
    {
        if (_overriddenSessionId == sessionId)
        {
            return;
        }

        Raise(new DetectionSessionOverridden(Id, sessionId));
    }
    private void When(DetectionSessionOverridden overridden)
    {
        _overriddenSessionId = overridden.SessionId;
    }

    public void OverridePilot(Guid pilotId)
    {
        if (_overriddenPilotId == pilotId)
        {
            return;
        }

        Raise(new DetectionPilotOverridden(Id, pilotId));
    }

    private void When(DetectionPilotOverridden overridden)
    {
        _overriddenPilotId = overridden.PilotId;
    }

    public void SetStatus(bool isValid)
    {
        if (_isValid == isValid)
        {
            return;
        }

        Raise(new DetectionStatusSet(Id, isValid));
    }

    private void When(DetectionStatusSet statusSet)
    {
        _isValid = statusSet.IsValid;
    }

    public void Move(long SoftwareTime, DateTime UtcTime)
    {
        if (!_canBeMoved)
        {
            throw new InvalidOperationException("Detection cannot be moved");
        }

        Raise(new DetectionMoved(Id, SoftwareTime, UtcTime));
    }
}
