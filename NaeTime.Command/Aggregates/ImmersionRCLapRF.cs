using EventDbLite.Aggregates;
using NaeTime.Events;
using System.Net;

namespace NaeTime.Command.Aggregates;
public class ImmersionRCLapRF : AggregateRoot
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
        public required Field<bool> IsEnabled { get; init; }
        public required Field<byte?> BandId { get; init; }
        public required Field<int> FrequencyInMHz { get; init; }
        public required Field<float> Threshold { get; init; }
        public required Field<ushort> Gain { get; init; }
    }

    private Type _type;
    private byte _lanes;

    private readonly Dictionary<byte, LaneInfo> _laneInfo = new();

    public ImmersionRCLapRF()
    {

    }

    public ImmersionRCLapRF(Guid id, string name, IPAddress address, ushort port, byte lanes)
    {
        Raise(new ImmersionRCLapRFNetworkDeviceRegistered(id,name, address.ToString(), port, lanes));
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

    public void RequestEnableLane(byte lane)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.IsEnabled.RequestedValue == true)
        {
            return; // Already requested
        }

        Raise(new ImmersionRCLapRFLaneEnableRequested(Id, lane));
    }

    private void When(ImmersionRCLapRFLaneEnableRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);

        laneInfo.IsEnabled.RequestedValue = true;
    }

    public void RequestDisableLane(byte lane)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.IsEnabled.RequestedValue == false)
        {
            return; // Already requested
        }

        Raise(new ImmersionRCLapRFLaneDisableRequested(Id, lane));
    }

    private void When(ImmersionRCLapRFLaneDisableRequested changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.RequestedValue = false;
    }

    public void ConfirmLaneEnabled(byte lane)
    {
        ThrowIfLaneNotExists(lane);

        Raise(new ImmersionRCLapRFLaneEnabled(Id, lane));
    }

    private void When(ImmersionRCLapRFLaneEnabled changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.ConfirmedValue = true;
    }

    public void ConfirmLaneDisabled(byte lane)
    {
        ThrowIfLaneNotExists(lane);

        Raise(new ImmersionRCLapRFLaneDisabled(Id, lane));
    }

    private void When(ImmersionRCLapRFLaneDisabled changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.IsEnabled.ConfirmedValue = false;
    }

    public void RequestTuneLaneVideoFrequency(byte lane, byte? bandId, int frequencyInMHz)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.BandId.RequestedValue == bandId && laneInfo.FrequencyInMHz.RequestedValue == frequencyInMHz)
        {
            return; // Already requested
        }

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
        if (laneInfo.BandId.ConfirmedValue == bandId && laneInfo.FrequencyInMHz.ConfirmedValue == frequencyInMHz)
        {
            return; // Already confirmed
        }

        Raise(new ImmersionRCLapRFLaneFrequencyTuned(Id, lane, bandId, frequencyInMHz));
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

        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.Threshold.RequestedValue == threshold)
        {
            return; // Already requested
        }

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
        if (laneInfo.Threshold.ConfirmedValue == threshold)
        {
            return; // Already confirmed
        }
        Raise(new ImmersionRCLapRFLaneThresholdConfigured(Id, lane, threshold));
    }

    private void When(ImmersionRCLapRFLaneThresholdConfigured changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Threshold.ConfirmedValue = changed.Threshold;
    }

    public void RequestLaneGain(byte lane, ushort gain)
    {
        ThrowIfLaneNotExists(lane);

        LaneInfo laneInfo = GetLaneInfo(lane);

        if (laneInfo.Gain.RequestedValue == gain)
        {
            return; // Already requested
        }

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

        if (laneInfo.Gain.ConfirmedValue == gain)
        {
            return; // Already confirmed
        }

        Raise(new ImmersionRCLapRFLaneGainConfigured(Id, lane, gain));
    }

    private void When(ImmersionRCLapRFLaneGainConfigured changed)
    {
        LaneInfo laneInfo = GetLaneInfo(changed.Lane);
        laneInfo.Gain.ConfirmedValue = changed.Gain;
    }

    public void MarkAsConnected()
    {
        Raise(new ImmersionRCLapRFTimerConnected(Id));
    }

    public void MarkAsDisconnected()
    {
        Raise(new ImmersionRCLapRFTimerDisconnected(Id));
    }

    public void RequestConfigurationConfirmation()
    {
        Raise(new ImmersionRCLapRFTimerRFSetupConfirmationRequested(Id));
    }

    public void ConfirmConfiguration()
    {
        Raise(new ImmersionRCLapRFTimerRFSetupConfirmed(Id));
    }

    public void MarkConfigurationMismatch()
    {
        Raise(new ImmersionRCLapRFTimerRFSetupMismatch(Id));
    }

    public void EnableRFSetupSync()
    {
        Raise(new ImmersionRCLapRFLaneRFSetupSyncEnabled(Id));
    }

    public void DisableRFSetupSync()
    {
        Raise(new ImmersionRCLapRFLaneRFSetupSyncDisabled(Id));
    }
}
