using NaeTime.Events;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;
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

    private void When(ImmersionRCLapRFNetworkDeviceRegistered register)
    {
        ImmersionRCLapRFLane[] lanes = new ImmersionRCLapRFLane[register.Lanes];
        for (byte i = 0; i < register.Lanes; i++)
        {
            lanes[i] = new ImmersionRCLapRFLane(i, false, 0, 0, null, 0);
        }

        Ethernet8ChannelImmersionRCLapRF timer = new(register.TimerId, register.Name, false, false, lanes, lanes, IPAddress.Parse(register.IPAddress), register.Port);
        _timers[register.TimerId] = timer;
    }
    private void When(ImmersionRCLapRFNetworkConfigurationChanged configured)
    {
        if (_timers.TryGetValue(configured.TimerId, out var timer) && timer is Ethernet8ChannelImmersionRCLapRF ethTimer)
        {
            _timers[configured.TimerId] = ethTimer with { IPAddress = IPAddress.Parse(configured.IPAddress), Port = configured.Port };
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
        if (_timers.TryGetValue(enabled.TimerId, out var timer))
        {
            _timers[enabled.TimerId].RequestedLanes[enabled.Lane] = _timers[enabled.TimerId].RequestedLanes[enabled.Lane] with { IsEnabled = true };
        }
    }
    private void When(ImmersionRCLapRFLaneDisableRequested disabled)
    {
        if (_timers.TryGetValue(disabled.TimerId, out var timer))
        {
            _timers[disabled.TimerId].RequestedLanes[disabled.Lane] = _timers[disabled.TimerId].RequestedLanes[disabled.Lane] with { IsEnabled = false };
        }
    }
    private void When(ImmersionRCLapRFLaneEnabled enabled)
    {
        if (_timers.TryGetValue(enabled.TimerId, out var timer))
        {
            _timers[enabled.TimerId].ConfirmedLanes[enabled.Lane] = _timers[enabled.TimerId].ConfirmedLanes[enabled.Lane] with { IsEnabled = true };
        }
    }
    private void When(ImmersionRCLapRFLaneDisabled disabled)
    {
        if (_timers.TryGetValue(disabled.TimerId, out var timer))
        {
            _timers[disabled.TimerId].ConfirmedLanes[disabled.Lane] = _timers[disabled.TimerId].ConfirmedLanes[disabled.Lane] with { IsEnabled = false };
        }
    }
    private void When(ImmersionRCLapRFLaneFrequencyRequested requested)
    {
        if (_timers.TryGetValue(requested.TimerId, out var timer))
        {
            _timers[requested.TimerId].RequestedLanes[requested.Lane] = _timers[requested.TimerId].RequestedLanes[requested.Lane] with { BandId = requested.BandId, FrequencyInMHz = requested.FrequencyInMHz };
        }
    }
    private void When(ImmersionRCLapRFLaneFrequencyTuned tuned)
    {
        if (_timers.TryGetValue(tuned.TimerId, out var timer))
        {
            _timers[tuned.TimerId].ConfirmedLanes[tuned.Lane] = _timers[tuned.TimerId].ConfirmedLanes[tuned.Lane] with { BandId = tuned.BandId, FrequencyInMHz = tuned.FrequencyInMHz };
        }
    }

    private void When(ImmersionRCLapRFLaneThresholdRequested requested)
    {
        if (_timers.TryGetValue(requested.TimerId, out var timer))
        {
            _timers[requested.TimerId].RequestedLanes[requested.Lane] = _timers[requested.TimerId].RequestedLanes[requested.Lane] with { Threshold = requested.Threshold };
        }
    }

    private void When(ImmersionRCLapRFLaneThresholdConfigured configured)
    {
        if (_timers.TryGetValue(configured.TimerId, out var timer))
        {
            _timers[configured.TimerId].ConfirmedLanes[configured.Lane] = _timers[configured.TimerId].ConfirmedLanes[configured.Lane] with { Threshold = configured.Threshold };
        }
    }
    private void When(ImmersionRCLapRFLaneGainRequested requested)
    {
        if (_timers.TryGetValue(requested.TimerId, out var timer))
        {
            _timers[requested.TimerId].RequestedLanes[requested.Lane] = _timers[requested.TimerId].RequestedLanes[requested.Lane] with { Gain = requested.Gain };
        }
    }
    private void When(ImmersionRCLapRFLaneGainConfigured configured)
    {
        if (_timers.TryGetValue(configured.TimerId, out var timer))
        {
            _timers[configured.TimerId].ConfirmedLanes[configured.Lane] = _timers[configured.TimerId].ConfirmedLanes[configured.Lane] with { Gain = configured.Gain };
        }
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
    private void When(ImmersionRCLapRFLaneRFSetupConfirmationRequested requested)
    {
        if (_timers.TryGetValue(requested.TimerId, out var timer))
        {
            _timers[requested.TimerId] = timer with { IsSetupConfirmed = false };
        }
    }

    private void When(ImmersionRCLapRFTimerRFSetupConfirmed confirmed)
    {
        if (_timers.TryGetValue(confirmed.TimerId, out var timer))
        {
            _timers[confirmed.TimerId] = timer with { IsSetupConfirmed = true };
        }
    }

}
