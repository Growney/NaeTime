using EventDbLite.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Events.Domain;
using NaeTime.Events.Integration;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;
using NaeTime.Reactions.Abstractions;

namespace NaeTime.Reactions;

public class TimingReactions
{
    private readonly IProjectionProvider _projectionProvider;
    private readonly IEventChannel _eventChannel;

    private PulledProjection<ISessionProjection>? _activeSessionProjection;
    private PulledProjection<ITimingProjection>? _timingProjection;
    private PulledProjection<ILaneConfigurationProjection>? _laneConfigurationProjection;

    public TimingReactions(IProjectionProvider projectionProvider, IEventChannel eventChannel)
    {
        _projectionProvider = projectionProvider;
        _eventChannel = eventChannel;
    }

    private Task When(HardwareDetectionOccured detection) => HandleDetection(detection.Id, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
    private Task When(DetectionManuallyTriggered detection) => HandleDetection(detection.Id, detection.Lane, null, detection.SoftwareTime, detection.UtcTime);
    private async Task HandleDetection(Guid detectionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var projection = await _projectionProvider.CloneOrPull(_activeSessionProjection);

        var activeSession = projection.Object.GetActiveSession();

        if (activeSession == null)
        {
            await _eventChannel.PublishAsync(new LiveDetectionOccurredWithNoActiveSession(detectionId));
            return;
        }

        await HandleSessionDetection(activeSession.Id, detectionId, activeSession.TrackId, lane, hardwareTime, softwareTime, utcTime);
    }
    private async Task HandleSessionDetection(Guid sessionId, Guid detectionId, Guid trackId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        await _eventChannel.PublishAsync(new LiveDetectionAssignedToSession(sessionId, detectionId));

        var projection = await _projectionProvider.CloneOrPull(_laneConfigurationProjection);

        var lanePilot = projection.Object.GetLanePilot(lane);

        if (!lanePilot.HasValue)
        {
            return;
        }

        await _eventChannel.PublishAsync(new LiveDetectionAssignedToPilot(sessionId, detectionId, lanePilot.Value));

        var timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var lastDetection = timingProjection.Object.GetLastPilotDetection(sessionId, trackId, lanePilot.Value);

        if(lastDetection?.Id != detectionId)
        {
            return;
        }

        await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(sessionId, detectionId, lanePilot.Value, utcTime));
    }
}
