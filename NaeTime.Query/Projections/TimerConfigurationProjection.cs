using NaeTime.Events;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;

namespace NaeTime.Query.Projections;
public class TimerConfigurationProjection : ITimerConfigurationProjection
{

    private class SessionLaneConfiguration
    {
        public required byte LaneNumber { get; init; }
        public byte? BandId { get; set; }
        public int FrequencyInMHz { get; set; }
        public bool IsEnabled { get; set; }
    }

    private class ImmersionRCLapRFLane
    {
        public required byte LaneNumber { get; init; }
        public float Threshold { get; set; }
        public ushort Gain { get; set; }
    }

    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, SessionLaneConfiguration>> _sessionLaneConfigurations = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, ImmersionRCLapRFLane>> _timerLaneConfigurations = new();
    private readonly ConcurrentDictionary<Guid, IEnumerable<Guid>> _sessionTimers = new();

    private Guid? _activeSession;

    private void When(OpenPracticeSessionActivated activated)
    {
        _activeSession = activated.SessionId;
    }
    private void When(OpenPracticeSessionDeactivated deactivated)
    {
        if (_activeSession == deactivated.SessionId)
        {
            _activeSession = null;
        }
    }
    private void When(OpenPracticeSessionScheduled scheduled)
    {
        _sessionTimers[scheduled.SessionId] = scheduled.TrackDetectors;
    }

    private void When(OpenPracticeSessionLaneVideoFrequencyTuned tuned)
    {
        ConcurrentDictionary<byte, SessionLaneConfiguration> sessionLanes = _sessionLaneConfigurations.GetOrAdd(tuned.SessionId, new ConcurrentDictionary<byte, SessionLaneConfiguration>());
        SessionLaneConfiguration laneConfig = sessionLanes.GetOrAdd(tuned.Lane, new SessionLaneConfiguration { LaneNumber = tuned.Lane });
        laneConfig.BandId = tuned.BandId;
        laneConfig.FrequencyInMHz = tuned.FrequencyInMHz;
    }
    private void When(OpenPracticeSessionLaneEnabled enabled)
    {
        ConcurrentDictionary<byte, SessionLaneConfiguration> sessionLanes = _sessionLaneConfigurations.GetOrAdd(enabled.SessionId, new ConcurrentDictionary<byte, SessionLaneConfiguration>());
        SessionLaneConfiguration laneConfig = sessionLanes.GetOrAdd(enabled.Lane, new SessionLaneConfiguration { LaneNumber = enabled.Lane });
        laneConfig.IsEnabled = true;
    }
    private void When(OpenPracticeSessionLaneDisabled disabled)
    {
        ConcurrentDictionary<byte, SessionLaneConfiguration> sessionLanes = _sessionLaneConfigurations.GetOrAdd(disabled.SessionId, new ConcurrentDictionary<byte, SessionLaneConfiguration>());
        SessionLaneConfiguration laneConfig = sessionLanes.GetOrAdd(disabled.Lane, new SessionLaneConfiguration { LaneNumber = disabled.Lane });
        laneConfig.IsEnabled = false;
    }
    private void When(ImmersionRCLapRFLaneThresholdConfigured configured)
    {
        ConcurrentDictionary<byte, ImmersionRCLapRFLane> timerLanes = _timerLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, ImmersionRCLapRFLane>());
        ImmersionRCLapRFLane laneConfig = timerLanes.GetOrAdd(configured.Lane, new ImmersionRCLapRFLane { LaneNumber = configured.Lane });
        laneConfig.Threshold = configured.Threshold;
    }
    private void When(ImmersionRCLapRFLaneGainConfigured configured)
    {
        ConcurrentDictionary<byte, ImmersionRCLapRFLane> timerLanes = _timerLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, ImmersionRCLapRFLane>());
        ImmersionRCLapRFLane laneConfig = timerLanes.GetOrAdd(configured.Lane, new ImmersionRCLapRFLane { LaneNumber = configured.Lane });
        laneConfig.Gain = configured.Gain;
    }

    public IEnumerable<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId)
    {
        if (!_activeSession.HasValue)
        {
            return Enumerable.Empty<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane>();
        }

        ConcurrentDictionary<byte, SessionLaneConfiguration>? sessionLanes = null;
        ConcurrentDictionary<byte, ImmersionRCLapRFLane>? timerLanes = null;

        if (!_sessionLaneConfigurations.TryGetValue(_activeSession.Value, out sessionLanes) && !_timerLaneConfigurations.TryGetValue(timerId, out timerLanes))
        {
            return Enumerable.Empty<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane>();
        }

        sessionLanes ??= new ConcurrentDictionary<byte, SessionLaneConfiguration>();
        timerLanes ??= new ConcurrentDictionary<byte, ImmersionRCLapRFLane>();

        if (!_sessionTimers.TryGetValue(_activeSession.Value, out var timers) || !timers.Contains(timerId))
        {
            return Enumerable.Empty<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane>();
        }

        List<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane> result = new();

        byte maxLanes = Math.Max(sessionLanes.Keys.Max(), timerLanes.Keys.Max());

        for (byte laneId = 0; laneId <= maxLanes; laneId++)
        {
            SessionLaneConfiguration? sessionLaneConfig = null;
            ImmersionRCLapRFLane? timerLaneConfig = null;
            if (!sessionLanes.TryGetValue(laneId, out sessionLaneConfig) && !timerLanes.TryGetValue(laneId, out timerLaneConfig))
            {
                continue;
            }

            result.Add(new NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane(
                laneId,
                sessionLaneConfig?.IsEnabled,
                timerLaneConfig?.Gain,
                timerLaneConfig?.Threshold,
                sessionLaneConfig?.BandId,
                sessionLaneConfig?.FrequencyInMHz
            ));
        }

        return result;
    }
}
