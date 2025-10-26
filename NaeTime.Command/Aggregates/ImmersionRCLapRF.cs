using EventDbLite.Aggregates;
using NaeTime.Events;
using System.Net;

namespace NaeTime.Command.Aggregates;
public class ImmersionRCLapRF : AggregateRoot<Guid>
{
    private enum Type
    {
        Network,
        Serial,
    }
    private class Field<T>
    {
        public T? RequestedValue { get; set; }
        public T? ConfirmedValue { get; set; }
    }
    private class LaneInfo
    {
        public required Field<bool?> IsEnabled { get; init; }
        public required Field<byte?> BandId { get; init; }
        public required Field<int?> FrequencyInMHz { get; init; }
        public required Field<float?> Threshold { get; init; }
        public required Field<ushort?> Gain { get; init; }
    }

    private Type _type;
    private byte _lanes;

    private readonly Dictionary<byte, LaneInfo> _laneInfo = [];

    public ImmersionRCLapRF()
    {

    }

    public ImmersionRCLapRF(Guid id, string name, IPAddress address, ushort port, byte lanes)
    {
        Raise(new ImmersionRCLapRFNetworkDeviceRegistered(id, name, address.ToString(), port, lanes));
    }

    private void When(ImmersionRCLapRFNetworkDeviceRegistered changed)
    {
        Id = changed.TimerId;
        _type = Type.Network;
        _lanes = changed.Lanes;
    }

    private LaneInfo GetLaneInfo(byte lane)
    {
        if (!_laneInfo.TryGetValue(lane, out LaneInfo? info))
        {
            info = new()
            {
                IsEnabled = new(),
                BandId = new(),
                FrequencyInMHz = new(),
                Threshold = new(),
                Gain = new()
            };
            _laneInfo[lane] = info;
        }

        return info;
    }

    public void ConfigureNetwork(IPAddress address, ushort port)
    {
        if (_type != Type.Network)
        {
            throw new InvalidOperationException("Cannot configure network settings for a non-network device.");
        }

        Raise(new ImmersionRCLapRFNetworkConfigurationChanged(Id, address.ToString(), port));
    }

    public void Rename(string name)
    {
        Raise(new ImmersionRCLapRFRenamed(Id, name));
    }

    private void ThrowIfLaneNotExists(byte lane)
    {
        if (lane < 0 || lane >= _lanes)
        {
            throw new ArgumentException("Lane does not exist.", nameof(lane));
        }
    }

    public void RequestLaneStatus(byte lane, bool desiredStatus)
    {
        ThrowIfLaneNotExists(lane);

        if (desiredStatus)
        {
            Raise(new ImmersionRCLapRFLaneEnableRequested(Id, lane));
        }
        else
        {
            Raise(new ImmersionRCLapRFLaneDisableRequested(Id, lane));
        }
    }

    private void When(ImmersionRCLapRFLaneEnableRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);

        laneInfo.IsEnabled.RequestedValue = true;
    }

    private void When(ImmersionRCLapRFLaneDisableRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.RequestedValue = false;
    }

    public void ConfirmLaneStatus(byte lane, bool status)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.IsEnabled.ConfirmedValue != status)
        {
            if (status)
            {
                Raise(new ImmersionRCLapRFLaneEnabled(Id, lane));
            }
            else
            {
                Raise(new ImmersionRCLapRFLaneDisabled(Id, lane));
            }
        }

        if (laneInfo.IsEnabled.RequestedValue.HasValue && laneInfo.IsEnabled.RequestedValue != status)
        {
            Raise(new ImmersionRCLapRFLaneStatusMismatch(Id, lane, laneInfo.IsEnabled.RequestedValue.Value, status));
        }
    }

    private void When(ImmersionRCLapRFLaneEnabled changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.ConfirmedValue = true;
    }
    private void When(ImmersionRCLapRFLaneDisabled changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.ConfirmedValue = false;
    }

    public void RequestTuneLaneVideoFrequency(byte lane, byte? bandId, int frequencyInMHz)
    {
        ThrowIfLaneNotExists(lane);

        Raise(new ImmersionRCLapRFLaneFrequencyRequested(Id, lane, bandId, frequencyInMHz));
    }

    private void When(ImmersionRCLapRFLaneFrequencyRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.BandId.RequestedValue = changed.BandId;
        laneInfo.FrequencyInMHz.RequestedValue = changed.FrequencyInMHz;
    }

    public void ConfirmLaneVideoFrequencyTuned(byte lane, byte? bandId, int frequencyInMHz)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.BandId.ConfirmedValue != bandId || laneInfo.FrequencyInMHz.ConfirmedValue != frequencyInMHz)
        {
            Raise(new ImmersionRCLapRFLaneFrequencyTuned(Id, lane, bandId, frequencyInMHz));
        }

        if (laneInfo.FrequencyInMHz.RequestedValue.HasValue && laneInfo.FrequencyInMHz.RequestedValue != frequencyInMHz)
        {
            Raise(new ImmersionRCLapRFLaneFrequencyMismatch(Id, lane, laneInfo.BandId.RequestedValue, laneInfo.FrequencyInMHz.RequestedValue.Value, bandId, frequencyInMHz));
        }
    }

    private void When(ImmersionRCLapRFLaneFrequencyTuned changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.BandId.ConfirmedValue = changed.BandId;
        laneInfo.FrequencyInMHz.ConfirmedValue = changed.FrequencyInMHz;
    }

    public void RequestLaneThreshold(byte lane, int threshold)
    {
        ThrowIfLaneNotExists(lane);

        Raise(new ImmersionRCLapRFLaneThresholdRequested(Id, lane, threshold));
    }

    private void When(ImmersionRCLapRFLaneThresholdRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Threshold.RequestedValue = changed.Threshold;
    }

    public void ConfirmThresholdConfigured(byte lane, int threshold)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.Threshold.ConfirmedValue != threshold)
        {
            Raise(new ImmersionRCLapRFLaneThresholdConfigured(Id, lane, threshold));
        }

        if (laneInfo.Threshold.RequestedValue.HasValue && laneInfo.Threshold.RequestedValue != threshold)
        {
            Raise(new ImmersionRCLapRFLaneThresholdMismatch(Id, lane, laneInfo.Threshold.RequestedValue.Value, threshold));
        }
    }

    private void When(ImmersionRCLapRFLaneThresholdConfigured changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Threshold.ConfirmedValue = changed.Threshold;
    }

    public void RequestLaneGain(byte lane, ushort gain)
    {
        ThrowIfLaneNotExists(lane);

        Raise(new ImmersionRCLapRFLaneGainRequested(Id, lane, gain));
    }

    private void When(ImmersionRCLapRFLaneGainRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Gain.RequestedValue = changed.Gain;
    }

    public void ConfirmLaneGainConfigured(byte lane, ushort gain)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.Gain.ConfirmedValue != gain)
        {
            Raise(new ImmersionRCLapRFLaneGainConfigured(Id, lane, gain));
        }

        if (laneInfo.Gain.RequestedValue.HasValue && laneInfo.Gain.RequestedValue != gain)
        {
            Raise(new ImmersionRCLapRFLaneGainMismatch(Id, lane, laneInfo.Gain.RequestedValue.Value, gain));
        }
    }

    private void When(ImmersionRCLapRFLaneGainConfigured changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Gain.ConfirmedValue = changed.Gain;
    }

    private void When(ImmersionRCLapRFConfigurationUnconfirmed _)
    {
        foreach (byte lane in _laneInfo.Keys)
        {
            LaneInfo laneInfo = GetLaneInfo(lane);
            laneInfo.IsEnabled.ConfirmedValue = null;
            laneInfo.BandId.ConfirmedValue = null;
            laneInfo.FrequencyInMHz.ConfirmedValue = null;
            laneInfo.Threshold.ConfirmedValue = null;
            laneInfo.Gain.ConfirmedValue = null;
        }
    }

    public void MarkAsConnected()
    {
        Raise(new ImmersionRCLapRFTimerConnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
    }
    public void MarkAsDisconnected()
    {
        Raise(new ImmersionRCLapRFTimerDisconnected(Id));
        Raise(new ImmersionRCLapRFConfigurationUnconfirmed(Id));
    }
}
