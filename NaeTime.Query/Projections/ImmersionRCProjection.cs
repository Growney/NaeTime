using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Query.Projections;
public class ImmersionRCProjection : IImmersionRCProjection
{
    private readonly ConcurrentDictionary<Guid, ImmersionRCLapRF> _timers = new();

    public Ethernet8ChannelImmersionRCLapRF? GetImmersionRCLapRF(Guid id)
    {
        if (!_timers.TryGetValue(id, out ImmersionRCLapRF? timer))
        {
            return null;
        }

        return timer as Ethernet8ChannelImmersionRCLapRF;
    }

    public IEnumerable<ImmersionRCLapRF> GetAllImmersionRCLapRFs()
    {
        return _timers.Values;
    }
    public ImmersionRCLapRFLane? GetImmersionRCLapRFLane(Guid timerId, byte laneId)
    {
        if (_timers.TryGetValue(timerId, out ImmersionRCLapRF? timer))
        {
            return timer.Lanes[laneId];
        }
        return null;
    }
    private void When(ImmersionRCLapRFNetworkDeviceRegistered register)
    {
        ImmersionRCLapRFLane[] lanes = new ImmersionRCLapRFLane[register.Lanes];
        for (byte i = 0; i < register.Lanes; i++)
        {
            lanes[i] = new ImmersionRCLapRFLane(i, new(false, false, false), new(null, null, false), new(null, null, false), new(null, null, false), new(0, 0, false));
        }

        Ethernet8ChannelImmersionRCLapRF timer = new(register.TimerId, register.Name, false, lanes, register.IPAddress, register.Port);
        _timers[register.TimerId] = timer;
    }
    private void When(ImmersionRCLapRFNetworkConfigurationChanged configured)
    {
        if (_timers.TryGetValue(configured.TimerId, out var timer) && timer is Ethernet8ChannelImmersionRCLapRF ethTimer)
        {
            _timers[configured.TimerId] = ethTimer with { IPAddress = configured.IPAddress, Port = configured.Port };
        }
    }
    private void When(ImmersionRCLapRFRenamed rename)
    {
        if (_timers.TryGetValue(rename.TimerId, out var timer))
        {
            _timers[rename.TimerId] = timer with { Name = rename.Name };
        }
    }
    private void When(ImmersionRCLapRFLaneEnableRequested enabled)
    {
        ImmersionRCLapRFLane lane = _timers[enabled.TimerId].Lanes[enabled.Lane];

        _timers[enabled.TimerId].Lanes[enabled.Lane] = lane with { IsEnabled = lane.IsEnabled with { Requested = true, ConfirmedMismatch = false } };

    }
    private void When(ImmersionRCLapRFLaneDisableRequested disabled)
    {
        ImmersionRCLapRFLane lane = _timers[disabled.TimerId].Lanes[disabled.Lane];

        _timers[disabled.TimerId].Lanes[disabled.Lane] = lane with { IsEnabled = lane.IsEnabled with { Requested = false, ConfirmedMismatch = false } };

    }
    private void When(ImmersionRCLapRFLaneEnabled enabled)
    {
        ImmersionRCLapRFLane lane = _timers[enabled.TimerId].Lanes[enabled.Lane];

        _timers[enabled.TimerId].Lanes[enabled.Lane] = lane with { IsEnabled = lane.IsEnabled with { Confirmed = true, ConfirmedMismatch = false } };

    }
    private void When(ImmersionRCLapRFLaneDisabled disabled)
    {
        ImmersionRCLapRFLane lane = _timers[disabled.TimerId].Lanes[disabled.Lane];

        _timers[disabled.TimerId].Lanes[disabled.Lane] = lane with { IsEnabled = lane.IsEnabled with { Confirmed = false, ConfirmedMismatch = false } };
    }

    private void When(ImmersionRCLapRFLaneStatusMismatch mismatch)
    {
        ImmersionRCLapRFLane lane = _timers[mismatch.TimerId].Lanes[mismatch.Lane];
        _timers[mismatch.TimerId].Lanes[mismatch.Lane] = lane with { IsEnabled = lane.IsEnabled with { Requested = mismatch.DesiredStatus, ConfirmedMismatch = true, Confirmed = mismatch.ActualStatus } };
    }

    private void When(ImmersionRCLapRFLaneFrequencyRequested requested)
    {
        ImmersionRCLapRFLane lane = _timers[requested.TimerId].Lanes[requested.Lane];
        _timers[requested.TimerId].Lanes[requested.Lane] = lane with { BandId = lane.BandId with { Requested = requested.BandId, ConfirmedMismatch = false }, FrequencyInMHz = lane.FrequencyInMHz with { Requested = requested.FrequencyInMHz, ConfirmedMismatch = false } };

    }
    private void When(ImmersionRCLapRFLaneFrequencyTuned tuned)
    {
        ImmersionRCLapRFLane lane = _timers[tuned.TimerId].Lanes[tuned.Lane];
        _timers[tuned.TimerId].Lanes[tuned.Lane] = lane with { BandId = lane.BandId with { Confirmed = tuned.BandId, ConfirmedMismatch = false }, FrequencyInMHz = lane.FrequencyInMHz with { Confirmed = tuned.FrequencyInMHz, ConfirmedMismatch = false } };
    }

    private void When(ImmersionRCLapRFLaneThresholdRequested requested)
    {
        ImmersionRCLapRFLane lane = _timers[requested.TimerId].Lanes[requested.Lane];
        _timers[requested.TimerId].Lanes[requested.Lane] = lane with { Threshold = lane.Threshold with { Requested = requested.Threshold, ConfirmedMismatch = false } };
    }

    private void When(ImmersionRCLapRFLaneThresholdConfigured configured)
    {
        ImmersionRCLapRFLane lane = _timers[configured.TimerId].Lanes[configured.Lane];
        _timers[configured.TimerId].Lanes[configured.Lane] = lane with { Threshold = lane.Threshold with { Confirmed = configured.Threshold, ConfirmedMismatch = false } };
    }

    private void When(ImmersionRCLapRFLaneThresholdMismatch mismatch)
    {
        ImmersionRCLapRFLane lane = _timers[mismatch.TimerId].Lanes[mismatch.Lane];
        _timers[mismatch.TimerId].Lanes[mismatch.Lane] = lane with { Threshold = lane.Threshold with { Confirmed = mismatch.ActualThreshold, Requested = mismatch.DesiredThreshold, ConfirmedMismatch = true } };
    }

    private void When(ImmersionRCLapRFLaneGainRequested requested)
    {
        ImmersionRCLapRFLane lane = _timers[requested.TimerId].Lanes[requested.Lane];
        _timers[requested.TimerId].Lanes[requested.Lane] = lane with { Gain = lane.Gain with { Requested = requested.Gain, ConfirmedMismatch = false } };
    }
    private void When(ImmersionRCLapRFLaneGainConfigured configured)
    {
        ImmersionRCLapRFLane lane = _timers[configured.TimerId].Lanes[configured.Lane];
        _timers[configured.TimerId].Lanes[configured.Lane] = lane with { Gain = lane.Gain with { Confirmed = configured.Gain, ConfirmedMismatch = false } };
    }

    private void When(ImmersionRCLapRFLaneGainMismatch mismatch)
    {
        ImmersionRCLapRFLane lane = _timers[mismatch.TimerId].Lanes[mismatch.Lane];
        _timers[mismatch.TimerId].Lanes[mismatch.Lane] = lane with { Gain = lane.Gain with { Confirmed = mismatch.ActualGain, Requested = mismatch.DesiredGain, ConfirmedMismatch = true } };
    }

    private void When(ImmersionRCLapRFTimerConnected connected)
    {
        if (_timers.TryGetValue(connected.TimerId, out var timer))
        {
            _timers[connected.TimerId] = timer with { IsConnected = true };
        }
    }
    private void When(ImmersionRCLapRFTimerDisconnected disconnected)
    {
        if (_timers.TryGetValue(disconnected.TimerId, out var timer))
        {
            _timers[disconnected.TimerId] = timer with { IsConnected = false };
        }
    }
}
