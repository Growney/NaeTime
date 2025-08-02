using EventDbLite.Aggregates;
using NaeTime.Events;

namespace NaeTime.Command.Aggregates;
public class NaeTimeNode : AggregateRoot
{
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
        public required Field<ushort> EntryThreshold { get; init; }
        public required Field<ushort> ExitThreshold { get; init; }
    }

    private string? _name;
    private string? _port;
    private readonly Dictionary<byte, LaneInfo> _laneInfo = new();

    public NaeTimeNode() { }

    public NaeTimeNode(Guid id, string name, string port)
    {
        Raise(new NaeTimeNodeSerialEsp32NodeConfigured(id, name, port));
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
                EntryThreshold = new(),
                ExitThreshold = new()
            };
            _laneInfo[lane] = info;
        }
        return info;
    }

    private void When(NaeTimeNodeSerialEsp32NodeConfigured e)
    {
        Id = e.TimerId;
        _name = e.Name;
        _port = e.Port;
    }

    public void RequestEnableLane(byte lane)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.IsEnabled.RequestedValue == true)
            return;
        Raise(new NaeTimeNodeLaneEnableRequested(Id, lane));
    }

    private void When(NaeTimeNodeLaneEnableRequested e)
    {
        GetLaneInfo(e.Lane).IsEnabled.RequestedValue = true;
    }

    public void RequestDisableLane(byte lane)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.IsEnabled.RequestedValue == false)
            return;
        Raise(new NaeTimeNodeLaneDisableRequested(Id, lane));
    }

    private void When(NaeTimeNodeLaneDisableRequested e)
    {
        GetLaneInfo(e.Lane).IsEnabled.RequestedValue = false;
    }

    public void ConfirmLaneEnabled(byte lane)
    {
        Raise(new NaeTimeNodeLaneEnabled(Id, lane));
    }

    private void When(NaeTimeNodeLaneEnabled e)
    {
        GetLaneInfo(e.Lane).IsEnabled.ConfirmedValue = true;
    }

    public void ConfirmLaneDisabled(byte lane)
    {
        Raise(new NaeTimeNodeLaneDisabled(Id, lane));
    }

    private void When(NaeTimeNodeLaneDisabled e)
    {
        GetLaneInfo(e.Lane).IsEnabled.ConfirmedValue = false;
    }

    public void RequestLaneFrequency(byte lane, byte? bandId, int frequencyInMHz)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.BandId.RequestedValue == bandId && laneInfo.FrequencyInMHz.RequestedValue == frequencyInMHz)
            return;
        Raise(new NaeTimeNodeLaneFrequencyRequested(Id, lane, bandId, frequencyInMHz));
    }

    private void When(NaeTimeNodeLaneFrequencyRequested e)
    {
        var lane = GetLaneInfo(e.Lane);
        lane.BandId.RequestedValue = e.BandId;
        lane.FrequencyInMHz.RequestedValue = e.FrequencyInMHz;
    }

    public void ConfirmLaneFrequencyTuned(byte lane, byte? bandId, int frequencyInMHz)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.BandId.ConfirmedValue == bandId && laneInfo.FrequencyInMHz.ConfirmedValue == frequencyInMHz)
            return;
        Raise(new NaeTimeNodeLaneFrequencyTuned(Id, lane, bandId, frequencyInMHz));
    }

    private void When(NaeTimeNodeLaneFrequencyTuned e)
    {
        var lane = GetLaneInfo(e.Lane);
        lane.BandId.ConfirmedValue = e.BandId;
        lane.FrequencyInMHz.ConfirmedValue = e.FrequencyInMHz;
    }

    public void RequestLaneEntryThreshold(byte lane, ushort threshold)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.EntryThreshold.RequestedValue == threshold)
            return;
        Raise(new NaeTimeNodeLaneEntryThresholdRequested(Id, lane, threshold));
    }

    private void When(NaeTimeNodeLaneEntryThresholdRequested e)
    {
        GetLaneInfo(e.Lane).EntryThreshold.RequestedValue = e.Threshold;
    }

    public void ConfirmLaneEntryThresholdConfigured(byte lane, ushort threshold)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.EntryThreshold.ConfirmedValue == threshold)
            return;
        Raise(new NaeTimeNodeLaneEntryThresholdConfigured(Id, lane, threshold));
    }

    private void When(NaeTimeNodeLaneEntryThresholdConfigured e)
    {
        GetLaneInfo(e.Lane).EntryThreshold.ConfirmedValue = e.Threshold;
    }

    public void RequestLaneExitThreshold(byte lane, ushort threshold)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.ExitThreshold.RequestedValue == threshold)
            return;
        Raise(new NaeTimeNodeLaneExitThresholdRequested(Id, lane, threshold));
    }

    private void When(NaeTimeNodeLaneExitThresholdRequested e)
    {
        GetLaneInfo(e.Lane).ExitThreshold.RequestedValue = e.Threshold;
    }

    public void ConfirmLaneExitThresholdConfigured(byte lane, ushort threshold)
    {
        LaneInfo laneInfo = GetLaneInfo(lane);
        if (laneInfo.ExitThreshold.ConfirmedValue == threshold)
            return;
        Raise(new NaeTimeNodeLaneExitThresholdConfigured(Id, lane, threshold));
    }

    private void When(NaeTimeNodeLaneExitThresholdConfigured e)
    {
        GetLaneInfo(e.Lane).ExitThreshold.ConfirmedValue = e.Threshold;
    }

    public void MarkLaneRFSetupRead(byte lane, bool isEnabled, byte? bandId, int frequencyInMHz, ushort entryThreshold, ushort exitThreshold)
    {
        Raise(new NaeTimeNodeLaneRFSetupRead(Id, lane, isEnabled, bandId, frequencyInMHz, entryThreshold, exitThreshold));
    }

    private void When(NaeTimeNodeLaneRFSetupRead e)
    {
        var lane = GetLaneInfo(e.Lane);
        lane.IsEnabled.ConfirmedValue = e.IsEnabled;
        lane.BandId.ConfirmedValue = e.BandId;
        lane.FrequencyInMHz.ConfirmedValue = e.FrequencyInMHz;
        lane.EntryThreshold.ConfirmedValue = e.EntryThreshold;
        lane.ExitThreshold.ConfirmedValue = e.ExitThreshold;
    }

    public void RequestLaneRFSetupConfirmation(byte lane)
    {
        Raise(new NaeTimeNodeLaneRFSetupConfirmationRequested(Id, lane));
    }

    public void ConfirmLaneRFSetup(byte lane)
    {
        Raise(new NaeTimeNodeLaneRFSetupConfirmed(Id, lane));
    }

    public void MarkLaneRFSetupMismatch(byte lane)
    {
        Raise(new NaeTimeNodeLaneRFSetupMismatch(Id, lane));
    }

    public void EnableLaneRFSetupSync(byte lane)
    {
        Raise(new NaeTimeNodeLaneRFSetupSyncEnabled(Id, lane));
    }

    public void DisableLaneRFSetupSync(byte lane)
    {
        Raise(new NaeTimeNodeLaneRFSetupSyncDisabled(Id, lane));
    }

    public void RequestTimerRFSetupConfirmation()
    {
        Raise(new NaeTimeNodeTimerRFSetupConfirmationRequested(Id));
    }

    public void ConfirmTimerRFSetup()
    {
        Raise(new NaeTimeNodeTimerRFSetupConfirmed(Id));
    }

    public void MarkTimerRFSetupMismatch()
    {
        Raise(new NaeTimeNodeTimerRFSetupMismatch(Id));
    }

    public void MarkAsConnected()
    {
        Raise(new NaeTimeNodeTimerConnected(Id));
    }

    public void MarkAsDisconnected()
    {
        Raise(new NaeTimeNodeTimerDisconnected(Id));
    }
}
