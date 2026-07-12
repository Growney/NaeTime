using EventDbLite.Aggregates;
using NaeTime.Events.Domain;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace NaeTime.Command.Aggregates;

public class OpenPracticeSessionLane : AggregateRoot<OpenPracticeSessionLane.LaneKey>
{
    public record LaneKey(Guid OpenPracticeSessionId, byte Lane)
    {
        public override string ToString() => $"{OpenPracticeSessionId}-{Lane}";
    }
    private Guid? _pilotId;
    private bool _isEnabled;
    private byte? _bandId;
    private int _frequencyInMHz;

    public OpenPracticeSessionLane()
    {

    }

    public OpenPracticeSessionLane(Guid sessionId, byte lane, byte? bandId, int frequencyInMHz)
    {
        Raise(new OpenPracticeSessionLaneConfigured(sessionId, lane, bandId, frequencyInMHz));
    }
    public OpenPracticeSessionLane(Guid oldSessionId, Guid newSessionId, byte lane, byte? bandId, int frequencyInMHz)
        :this(newSessionId,lane,bandId,frequencyInMHz)
    {
        Raise(new OpenPracticeSessionLaneCloneCreated(newSessionId, oldSessionId, lane));
    }

    private void When(OpenPracticeSessionLaneConfigured configured)
    {
        Id = new LaneKey(configured.SessionId, configured.Lane);
        _bandId = configured.BandId;
        _frequencyInMHz = configured.FrequencyInMHz;
    }

    public void EnableLane()
    {
        ThrowIfIdNotSet();
        if (_isEnabled)
        {
            return;
        }
        Raise(new OpenPracticeSessionLaneStatusSet(Id.OpenPracticeSessionId, Id.Lane,true));
    }
    public void DisableLane()
    {
        ThrowIfIdNotSet();
        if (!_isEnabled)
        {
            return;
        }
        Raise(new OpenPracticeSessionLaneStatusSet(Id.OpenPracticeSessionId, Id.Lane, false));
    }
    private void When(OpenPracticeSessionLaneStatusSet statusSet)
    {
        _isEnabled = statusSet.IsEnabled;
    }
    public void TuneLaneVideoFrequency(byte? bandId, int frequencyInMHz)
    {
        ThrowIfIdNotSet();
        if (bandId == _bandId && frequencyInMHz == _frequencyInMHz)
        {
            return;
        }
        Raise(new OpenPracticeSessionLaneVideoFrequencyTuned(Id.OpenPracticeSessionId, Id.Lane, bandId, frequencyInMHz));
    }
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned laneVideoFrequencyTuned)
    {
        _bandId = laneVideoFrequencyTuned.BandId;
        _frequencyInMHz = laneVideoFrequencyTuned.FrequencyInMHz;
    }
    public void ResetLanePilot()
    {
        ThrowIfIdNotSet();
        if (!_pilotId.HasValue)
        {
            return;
        }
        Raise(new OpenPracticeSessionLanePilotReset(Id.OpenPracticeSessionId, Id.Lane));      
    }
    public void SetLanePilot(Guid pilotId)
    {
        ThrowIfIdNotSet();
        if(_pilotId == pilotId)
        {
            return;
        }
        Raise(new OpenPracticeSessionLanePilotSet(Id.OpenPracticeSessionId, Id.Lane, pilotId));
    }

    private void When(OpenPracticeSessionLanePilotSet lanePilotSet)
    {
        _pilotId = lanePilotSet.PilotId;
    }
    private void When(OpenPracticeSessionLanePilotReset _)
    {
        _pilotId = null;
    }
}
