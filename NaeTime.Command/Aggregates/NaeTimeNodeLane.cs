using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class NaeTimeNodeLane : AggregateRoot<NaeTimeNodeLane.NaeTimeNodeLaneId>
{
    private enum LaneStatus
    {
        Mismatched,
        Pending,
        Confirmed,
    }

    public record NaeTimeNodeLaneId(Guid SessionId, byte LaneId)
    {
        public override string ToString() => $"{SessionId}-{LaneId}";

    }

    private class Field<T>
    {
        public T? RequestedValue { get; set; }
        public T? ConfirmedValue { get; set; }
    }

    private Field<bool?> _isEnabled = new();
    private Field<byte?> _bandId = new();
    private Field<int?> _frequencyInMHz = new();
    private Field<ushort?> _entryThreshold = new();
    private Field<ushort?> _exitThreshold = new();

    private LaneStatus _status = LaneStatus.Pending;

    public NaeTimeNodeLane() { }

    public NaeTimeNodeLane(Guid timerId, byte laneId)
    {
        Raise(new NaeTimeNodeRFLaneCreated(timerId, laneId));
    }

    private void When(NaeTimeNodeRFLaneCreated created)
    {
        Id = new NaeTimeNodeLaneId(created.TimerId, created.LaneId);
    }

    private void When(NaeTimeNodeLaneEnableRequested _) => _isEnabled.RequestedValue = true;
    private void When(NaeTimeNodeLaneDisableRequested _) => _isEnabled.RequestedValue = false;

    private void When(NaeTimeNodeLaneEnabled _) => _isEnabled.ConfirmedValue = true;
    private void When(NaeTimeNodeLaneDisabled _) => _isEnabled.ConfirmedValue = false;

    private void When(NaeTimeNodeLaneFrequencyRequested changed)
    {
        _bandId.RequestedValue = changed.BandId;
        _frequencyInMHz.RequestedValue = changed.FrequencyInMHz;
    }

    private void When(NaeTimeNodeLaneFrequencyTuned changed)
    {
        _bandId.ConfirmedValue = changed.BandId;
        _frequencyInMHz.ConfirmedValue = changed.FrequencyInMHz;
    }

    private void When(NaeTimeNodeLaneEntryThresholdRequested changed)
    {
        _entryThreshold.RequestedValue = changed.Threshold;
    }

    private void When(NaeTimeNodeLaneEntryThresholdConfigured changed)
    {
        _entryThreshold.ConfirmedValue = changed.Threshold;
    }

    private void When(NaeTimeNodeLaneExitThresholdRequested changed)
    {
        _exitThreshold.RequestedValue = changed.Threshold;
    }

    private void When(NaeTimeNodeLaneExitThresholdConfigured changed)
    {
        _exitThreshold.ConfirmedValue = changed.Threshold;
    }

    private void When(NaeTimeNodeConfigurationUnconfirmed _)
    {
        _isEnabled.ConfirmedValue = null;
        _bandId.ConfirmedValue = null;
        _frequencyInMHz.ConfirmedValue = null;
        _entryThreshold.ConfirmedValue = null;
        _exitThreshold.ConfirmedValue = null;
    }

    private void When(TimerLaneMismatch _) => _status = LaneStatus.Mismatched;
    private void When(TimerLanePending _) => _status = LaneStatus.Pending;
    private void When(TimerLaneConfirmed _) => _status = LaneStatus.Confirmed;

    private void CheckAndRaiseMismatch()
    {
        ThrowIfIdNotSet();
        if (_status != LaneStatus.Mismatched)
        {
            Raise(new TimerLaneMismatch(Id.SessionId, Id.LaneId));
        }
    }

    private void CheckAndRaisePending()
    {
        ThrowIfIdNotSet();
        if (_status != LaneStatus.Pending)
        {
            Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
        }
    }

    private void CheckAndRaiseConfirmed()
    {
        ThrowIfIdNotSet();
        if (_status != LaneStatus.Confirmed)
        {
            Raise(new TimerLaneConfirmed(Id.SessionId, Id.LaneId));
        }
    }

    public void ConfirmLaneStatus(bool status)
    {
        ThrowIfIdNotSet();

        if (_isEnabled.ConfirmedValue != status)
        {
            if (status)
            {
                Raise(new NaeTimeNodeLaneEnabled(Id.SessionId, Id.LaneId));
            }
            else
            {
                Raise(new NaeTimeNodeLaneDisabled(Id.SessionId, Id.LaneId));
            }
        }

        if (_isEnabled.RequestedValue.HasValue && _isEnabled.RequestedValue != status)
        {
            Raise(new NaeTimeNodeLaneStatusMismatch(Id.SessionId, Id.LaneId, _isEnabled.RequestedValue.Value, status));
            CheckAndRaiseMismatch();
        }
        else
        {
            CheckAndRaiseConfirmed();
        }
    }

    public void RequestLaneStatus(bool desiredStatus)
    {
        ThrowIfIdNotSet();

        if (desiredStatus)
        {
            Raise(new NaeTimeNodeLaneEnableRequested(Id.SessionId, Id.LaneId));
            CheckAndRaisePending();
        }
        else
        {
            Raise(new NaeTimeNodeLaneDisableRequested(Id.SessionId, Id.LaneId));
            CheckAndRaisePending();
        }
    }

    public void RequestLaneFrequency(byte? bandId, int frequencyInMHz)
    {
        ThrowIfIdNotSet();
        Raise(new NaeTimeNodeLaneFrequencyRequested(Id.SessionId, Id.LaneId, bandId, frequencyInMHz));
        CheckAndRaisePending();
    }

    public void ConfirmLaneFrequencyTuned(byte? bandId, int frequencyInMHz)
    {
        ThrowIfIdNotSet();

        if (_bandId.ConfirmedValue != bandId || _frequencyInMHz.ConfirmedValue != frequencyInMHz)
        {
            Raise(new NaeTimeNodeLaneFrequencyTuned(Id.SessionId, Id.LaneId, bandId, frequencyInMHz));
        }

        if (_frequencyInMHz.RequestedValue.HasValue && _frequencyInMHz.RequestedValue != frequencyInMHz)
        {
            Raise(new NaeTimeNodeLaneFrequencyMismatch(Id.SessionId, Id.LaneId, _bandId.RequestedValue, _frequencyInMHz.RequestedValue.Value, bandId, frequencyInMHz));
            CheckAndRaiseMismatch();
        }
        else
        {
            CheckAndRaiseConfirmed();
        }
    }

    public void RequestLaneEntryThreshold(ushort threshold)
    {
        ThrowIfIdNotSet();
        Raise(new NaeTimeNodeLaneEntryThresholdRequested(Id.SessionId, Id.LaneId, threshold));
        Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
    }

    public void ConfirmLaneEntryThresholdConfigured(ushort threshold)
    {
        ThrowIfIdNotSet();

        if (_entryThreshold.ConfirmedValue != threshold)
        {
            Raise(new NaeTimeNodeLaneEntryThresholdConfigured(Id.SessionId, Id.LaneId, threshold));
        }

        if (_entryThreshold.RequestedValue.HasValue && _entryThreshold.RequestedValue != threshold)
        {
            Raise(new NaeTimeNodeLaneEntryThresholdMismatch(Id.SessionId, Id.LaneId, _entryThreshold.RequestedValue.Value, threshold));
            CheckAndRaiseMismatch();
        }
        else
        {
            CheckAndRaiseConfirmed();
        }
    }

    public void RequestLaneExitThreshold(ushort threshold)
    {
        ThrowIfIdNotSet();
        Raise(new NaeTimeNodeLaneExitThresholdRequested(Id.SessionId, Id.LaneId, threshold));
        Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
    }

    public void ConfirmLaneExitThresholdConfigured(ushort threshold)
    {
        ThrowIfIdNotSet();

        if (_exitThreshold.ConfirmedValue != threshold)
        {
            Raise(new NaeTimeNodeLaneExitThresholdConfigured(Id.SessionId, Id.LaneId, threshold));
        }

        if (_exitThreshold.RequestedValue.HasValue && _exitThreshold.RequestedValue != threshold)
        {
            Raise(new NaeTimeNodeLaneExitThresholdMismatch(Id.SessionId, Id.LaneId, _exitThreshold.RequestedValue.Value, threshold));
            CheckAndRaiseMismatch();
        }
        else
        {
            CheckAndRaiseConfirmed();
        }
    }
}
