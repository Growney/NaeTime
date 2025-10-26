using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class ImmersionRCLapRFLane : AggregateRoot<ImmersionRCLapRFLane.ImmersionRCLapRFLaneId>
{
    public record ImmersionRCLapRFLaneId(Guid SessionId, byte LaneId);
    private class Field<T>
    {
        public T? RequestedValue { get; set; }
        public T? ConfirmedValue { get; set; }
    }
    private Field<bool?> _isEnabled = new();
    private Field<byte?> _bandId = new();
    private Field<int?> _frequencyInMHz = new();
    private Field<float?> _threshold = new();
    private Field<ushort?> _gain = new();

    public ImmersionRCLapRFLane(Guid timerId, byte laneId)
    {
        Raise(new ImmersionRCLapRFLaneCreated(timerId, laneId));
    }
    public ImmersionRCLapRFLane()
    {
    }
    private void When(ImmersionRCLapRFLaneCreated created)
    {
        Id = new ImmersionRCLapRFLaneId(created.TimerId, created.LaneId);
    }
    private void When(ImmersionRCLapRFLaneEnableRequested changed)
    {
        _isEnabled.RequestedValue = true;
    }
    private void When(ImmersionRCLapRFLaneDisableRequested changed)
    {
        _isEnabled.RequestedValue = false;
    }
    private void When(ImmersionRCLapRFLaneEnabled changed)
    {
        _isEnabled.ConfirmedValue = true;
    }
    private void When(ImmersionRCLapRFLaneDisabled changed)
    {
        _isEnabled.ConfirmedValue = false;
    }
    private void When(ImmersionRCLapRFLaneFrequencyRequested changed)
    {
        _bandId.RequestedValue = changed.BandId;
        _frequencyInMHz.RequestedValue = changed.FrequencyInMHz;
    }
    private void When(ImmersionRCLapRFLaneFrequencyTuned changed)
    {
        _bandId.ConfirmedValue = changed.BandId;
        _frequencyInMHz.ConfirmedValue = changed.FrequencyInMHz;
    }
    private void When(ImmersionRCLapRFLaneThresholdRequested changed)
    {
        _threshold.RequestedValue = changed.Threshold;
    }
    private void When(ImmersionRCLapRFLaneThresholdConfigured changed)
    {
        _threshold.ConfirmedValue = changed.Threshold;
    }

    private void When(ImmersionRCLapRFLaneGainRequested changed)
    {
        _gain.RequestedValue = changed.Gain;
    }
    private void When(ImmersionRCLapRFLaneGainConfigured changed)
    {
        _gain.ConfirmedValue = changed.Gain;
    }

    private void When(ImmersionRCLapRFConfigurationUnconfirmed _)
    {
        _isEnabled.ConfirmedValue = null;
        _bandId.ConfirmedValue = null;
        _frequencyInMHz.ConfirmedValue = null;
        _threshold.ConfirmedValue = null;
        _gain.ConfirmedValue = null;
    }
    public void ConfirmLaneStatus(bool status)
    {
        ThrowIfIdNotSet();

        if (_isEnabled.ConfirmedValue != status)
        {
            if (status)
            {
                Raise(new ImmersionRCLapRFLaneEnabled(Id.SessionId, Id.LaneId));
            }
            else
            {
                Raise(new ImmersionRCLapRFLaneDisabled(Id.SessionId, Id.LaneId));
            }
        }

        if (_isEnabled.RequestedValue.HasValue && _isEnabled.RequestedValue != status)
        {
            Raise(new ImmersionRCLapRFLaneStatusMismatch(Id.SessionId, Id.LaneId, _isEnabled.RequestedValue.Value, status));
            Raise(new TimerLaneMismatch(Id.SessionId, Id.LaneId));
        }
        else
        {
            Raise(new TimerLaneConfirmed(Id.SessionId, Id.LaneId));
        }
    }
    public void RequestLaneStatus(bool desiredStatus)
    {
        ThrowIfIdNotSet();

        if (desiredStatus)
        {
            Raise(new ImmersionRCLapRFLaneEnableRequested(Id.SessionId, Id.LaneId));
            Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
        }
        else
        {
            Raise(new ImmersionRCLapRFLaneDisableRequested(Id.SessionId, Id.LaneId));
            Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
        }
    }
    public void RequestTuneLaneVideoFrequency(byte? bandId, int frequencyInMHz)
    {
        ThrowIfIdNotSet();

        Raise(new ImmersionRCLapRFLaneFrequencyRequested(Id.SessionId, Id.LaneId, bandId, frequencyInMHz));
        Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
    }
    public void ConfirmLaneVideoFrequencyTuned(byte? bandId, int frequencyInMHz)
    {
        ThrowIfIdNotSet();

        if (_bandId.ConfirmedValue != bandId || _frequencyInMHz.ConfirmedValue != frequencyInMHz)
        {
            Raise(new ImmersionRCLapRFLaneFrequencyTuned(Id.SessionId, Id.LaneId, bandId, frequencyInMHz));
        }

        if (_frequencyInMHz.RequestedValue.HasValue && _frequencyInMHz.RequestedValue != frequencyInMHz)
        {
            Raise(new ImmersionRCLapRFLaneFrequencyMismatch(Id.SessionId, Id.LaneId, _bandId.RequestedValue, _frequencyInMHz.RequestedValue.Value, bandId, frequencyInMHz));
            Raise(new TimerLaneMismatch(Id.SessionId, Id.LaneId));
        }
        else
        {
            Raise(new TimerLaneConfirmed(Id.SessionId, Id.LaneId));
        }
    }
    public void RequestLaneThreshold(float threshold)
    {
        ThrowIfIdNotSet();

        Raise(new ImmersionRCLapRFLaneThresholdRequested(Id.SessionId, Id.LaneId, threshold));
        Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
    }

    public void ConfirmThresholdConfigured(float threshold)
    {
        ThrowIfIdNotSet();

        if (_threshold.ConfirmedValue != threshold)
        {
            Raise(new ImmersionRCLapRFLaneThresholdConfigured(Id.SessionId, Id.LaneId, threshold));
        }

        if (_threshold.RequestedValue.HasValue && _threshold.RequestedValue != threshold)
        {
            Raise(new ImmersionRCLapRFLaneThresholdMismatch(Id.SessionId, Id.LaneId, _threshold.RequestedValue.Value, threshold));
            Raise(new TimerLaneMismatch(Id.SessionId, Id.LaneId));
        }
        else
        {
            Raise(new TimerLaneConfirmed(Id.SessionId, Id.LaneId));
        }
    }
    public void RequestLaneGain(ushort gain)
    {
        ThrowIfIdNotSet();

        Raise(new ImmersionRCLapRFLaneGainRequested(Id.SessionId, Id.LaneId, gain));
        Raise(new TimerLanePending(Id.SessionId, Id.LaneId));
    }

    public void ConfirmLaneGainConfigured(ushort gain)
    {
        ThrowIfIdNotSet();

        if (_gain.ConfirmedValue != gain)
        {
            Raise(new ImmersionRCLapRFLaneGainConfigured(Id.SessionId, Id.LaneId, gain));
        }

        if (_gain.RequestedValue.HasValue && _gain.RequestedValue != gain)
        {
            Raise(new ImmersionRCLapRFLaneGainMismatch(Id.SessionId, Id.LaneId, _gain.RequestedValue.Value, gain));
            Raise(new TimerLaneMismatch(Id.SessionId, Id.LaneId));
        }
        else
        {
            Raise(new TimerLaneConfirmed(Id.SessionId, Id.LaneId));
        }
    }
}
