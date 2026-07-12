using NaeTime.Events.Domain;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using System.Collections.Concurrent;
using System.ComponentModel.Design;

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

    private class NaeTimeNodeLaneInfo
    {
        public required byte LaneNumber { get; init; }
        public ushort? EntryThreshold { get; set; }
        public ushort? ExitThreshold { get; set; }
    }

    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, SessionLaneConfiguration>> _sessionLaneConfigurations = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, ImmersionRCLapRFLane>> _immersionRCTimerLaneConfigurations = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>> _naeTimeNodeLaneConfigurations = new();

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
    private void When(OpenPracticeSessionLaneVideoFrequencyTuned tuned)
    {
        ConcurrentDictionary<byte, SessionLaneConfiguration> sessionLanes = _sessionLaneConfigurations.GetOrAdd(tuned.SessionId, new ConcurrentDictionary<byte, SessionLaneConfiguration>());
        SessionLaneConfiguration laneConfig = sessionLanes.GetOrAdd(tuned.Lane, new SessionLaneConfiguration { LaneNumber = tuned.Lane });
        laneConfig.BandId = tuned.BandId;
        laneConfig.FrequencyInMHz = tuned.FrequencyInMHz;
    }
    private void When(OpenPracticeSessionLaneStatusSet status)
    {
        ConcurrentDictionary<byte, SessionLaneConfiguration> sessionLanes = _sessionLaneConfigurations.GetOrAdd(status.SessionId, new ConcurrentDictionary<byte, SessionLaneConfiguration>());
        SessionLaneConfiguration laneConfig = sessionLanes.GetOrAdd(status.Lane, new SessionLaneConfiguration { LaneNumber = status.Lane });
        laneConfig.IsEnabled = status.IsEnabled;
    }
    private void When(ImmersionRCLapRFLaneThresholdConfigured configured)
    {
        ConcurrentDictionary<byte, ImmersionRCLapRFLane> timerLanes = _immersionRCTimerLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, ImmersionRCLapRFLane>());
        ImmersionRCLapRFLane laneConfig = timerLanes.GetOrAdd(configured.Lane, new ImmersionRCLapRFLane { LaneNumber = configured.Lane });
        laneConfig.Threshold = configured.Threshold;
    }
    private void When(ImmersionRCLapRFLaneGainConfigured configured)
    {
        ConcurrentDictionary<byte, ImmersionRCLapRFLane> timerLanes = _immersionRCTimerLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, ImmersionRCLapRFLane>());
        ImmersionRCLapRFLane laneConfig = timerLanes.GetOrAdd(configured.Lane, new ImmersionRCLapRFLane { LaneNumber = configured.Lane });
        laneConfig.Gain = configured.Gain;
    }

    private void When(NaeTimeNodeLaneEntryThresholdConfigured configured)
    {
        var timerLanes = _naeTimeNodeLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>());
        var lane = timerLanes.GetOrAdd(configured.Lane, new NaeTimeNodeLaneInfo { LaneNumber = configured.Lane });
        lane.EntryThreshold = configured.Threshold;
    }

    private void When(NaeTimeNodeLaneExitThresholdConfigured configured)
    {
        var timerLanes = _naeTimeNodeLaneConfigurations.GetOrAdd(configured.TimerId, new ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>());
        var lane = timerLanes.GetOrAdd(configured.Lane, new NaeTimeNodeLaneInfo { LaneNumber = configured.Lane });
        lane.ExitThreshold = configured.Threshold;
    }

    private void When(NaeTimeNodeLaneFrequencyTuned tuned)
    {
        var timerLanes = _naeTimeNodeLaneConfigurations.GetOrAdd(tuned.TimerId, new ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>());
        var lane = timerLanes.GetOrAdd(tuned.Lane, new NaeTimeNodeLaneInfo { LaneNumber = tuned.Lane });
        // frequency and band are stored at session level for desired config; keep nothing here
    }

    private void When(NaeTimeNodeLaneEnabled enabled)
    {
        var timerLanes = _naeTimeNodeLaneConfigurations.GetOrAdd(enabled.TimerId, new ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>());
        var lane = timerLanes.GetOrAdd(enabled.Lane, new NaeTimeNodeLaneInfo { LaneNumber = enabled.Lane });
        // Node-level enabled state isn't treated as desired; session controls desired IsEnabled. Keep thresholds only.
    }

    public IEnumerable<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane> GetActiveImmersionRCLapRFLanesConfiguration(Guid timerId)
    {
        if (!_activeSession.HasValue)
        {
            return Enumerable.Empty<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane>();
        }

        ConcurrentDictionary<byte, SessionLaneConfiguration>? sessionLanes = null;
        ConcurrentDictionary<byte, ImmersionRCLapRFLane>? timerLanes = null;

        if (!_sessionLaneConfigurations.TryGetValue(_activeSession.Value, out sessionLanes) && !_immersionRCTimerLaneConfigurations.TryGetValue(timerId, out timerLanes))
        {
            return Enumerable.Empty<NaeTime.Query.Abstractions.Models.DesiredImmersionRCLapRFLane>();
        }

        sessionLanes ??= new ConcurrentDictionary<byte, SessionLaneConfiguration>();
        timerLanes ??= new ConcurrentDictionary<byte, ImmersionRCLapRFLane>();

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

    public IEnumerable<DesiredNaeTimeNodeLane> GetActiveNaeTimeNodeLanesConfiguration(Guid timerId)
    {
        if (!_activeSession.HasValue)
        {
            return Enumerable.Empty<DesiredNaeTimeNodeLane>();
        }

        _sessionLaneConfigurations.TryGetValue(_activeSession.Value, out var sessionLanes);
        _naeTimeNodeLaneConfigurations.TryGetValue(timerId, out var timerLanes);

        sessionLanes ??= new ConcurrentDictionary<byte, SessionLaneConfiguration>();
        timerLanes ??= new ConcurrentDictionary<byte, NaeTimeNodeLaneInfo>();

        if (!sessionLanes.Any() && !timerLanes.Any())
        {
            return Enumerable.Empty<DesiredNaeTimeNodeLane>();
        }

        byte maxLanes = 0;
        if (sessionLanes.Any()) maxLanes = Math.Max(maxLanes, sessionLanes.Keys.Max());
        if (timerLanes.Any()) maxLanes = Math.Max(maxLanes, timerLanes.Keys.Max());

        var result = new List<DesiredNaeTimeNodeLane>();

        for (byte laneId = 0; laneId <= maxLanes; laneId++)
        {
            sessionLanes.TryGetValue(laneId, out var sessionLaneConfig);
            timerLanes.TryGetValue(laneId, out var timerLaneConfig);

            if (sessionLaneConfig == null && timerLaneConfig == null)
                continue;

            result.Add(new DesiredNaeTimeNodeLane(
                laneId,
                sessionLaneConfig?.IsEnabled,
                timerLaneConfig?.EntryThreshold,
                timerLaneConfig?.ExitThreshold,
                sessionLaneConfig?.BandId,
                sessionLaneConfig?.FrequencyInMHz
            ));
        }

        return result;
    }
}
