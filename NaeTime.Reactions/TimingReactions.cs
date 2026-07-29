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

    private PulledAllStreamProjection<ISessionProjection>? _sessionProjection;
    private PulledAllStreamProjection<ITimingProjection>? _timingProjection;
    private PulledAllStreamProjection<ILaneConfigurationProjection>? _laneConfigurationProjection;

    public TimingReactions(IProjectionProvider projectionProvider, IEventChannel eventChannel)
    {
        _projectionProvider = projectionProvider;
        _eventChannel = eventChannel;
    }

    private Task When(HardwareDetectionOccured detection) => HandleDetection(detection.Id, detection.Lane, detection.UtcTime);
    private Task When(DetectionManuallyTriggered detection) => HandleDetection(detection.Id, detection.Lane, detection.UtcTime);
    private async Task HandleDetection(Guid detectionId, byte lane, DateTime utcTime)
    {
        _sessionProjection = await _projectionProvider.CloneOrPull(_sessionProjection);

        var activeSession = _sessionProjection.Object.GetActiveSession();

        if (activeSession == null)
        {
            await _eventChannel.PublishAsync(new LiveDetectionOccurredWithNoActiveSession(detectionId));
            return;
        }

        await HandleSessionDetection(activeSession.Id, detectionId, activeSession.TrackId, lane, utcTime);
    }
    private async Task HandleSessionDetection(Guid sessionId, Guid detectionId, Guid trackId, byte lane, DateTime utcTime)
    {
        await _eventChannel.PublishAsync(new LiveDetectionAssignedToSession(sessionId, detectionId));

        _laneConfigurationProjection = await _projectionProvider.CloneOrPull(_laneConfigurationProjection);

        var lanePilot = _laneConfigurationProjection.Object.GetLanePilot(lane);

        if (!lanePilot.HasValue)
        {
            return;
        }

        await _eventChannel.PublishAsync(new LiveDetectionAssignedToPilot(sessionId, trackId, detectionId, lanePilot.Value));

        _timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var lastDetection = _timingProjection.Object.GetLastPilotDetection(sessionId, trackId, lanePilot.Value);

        if(lastDetection?.Id != detectionId)
        {
            return;
        }

        await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(sessionId, trackId, detectionId, lanePilot.Value, utcTime));
    }

    private async Task When(DetectionPilotOverridden detection)
    {
        var previousTimingProjection = _timingProjection;
        var previousDetection = previousTimingProjection?.Object.GetDetection(detection.Id);

        _timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var currentDetection = _timingProjection.Object.GetDetection(detection.Id);
        if (currentDetection is null)
        {
            return;
        }

        var previousPilotId = previousDetection?.PilotId;
        var newPilotId = currentDetection.PilotId;

        var previousContext = await ResolveSessionAndTrack(previousDetection?.SessionId, previousDetection?.TrackId);
        var currentContext = await ResolveSessionAndTrack(currentDetection.SessionId, currentDetection.TrackId);

        if (!newPilotId.HasValue || !currentContext.HasValue || previousPilotId == newPilotId)
        {
            return;
        }

        if (previousPilotId.HasValue && previousContext.HasValue)
        {
            var previousLastDetection = previousTimingProjection?.Object.GetLastPilotDetection(previousContext.Value.sessionId, previousContext.Value.trackId, previousPilotId.Value);
            if (previousLastDetection?.Id == detection.Id)
            {
                var oldPilotNewLastDetection = _timingProjection.Object.GetLastPilotDetection(previousContext.Value.sessionId, previousContext.Value.trackId, previousPilotId.Value);
                if (oldPilotNewLastDetection is null)
                {
                    await _eventChannel.PublishAsync(new PilotsMostRecentDetectionCleared(previousContext.Value.sessionId, previousContext.Value.trackId, previousPilotId.Value));
                }
                else
                {
                    await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(previousContext.Value.sessionId, previousContext.Value.trackId, oldPilotNewLastDetection.Id, previousPilotId.Value, oldPilotNewLastDetection.UtcTime));
                }
            }

            await _eventChannel.PublishAsync(new PilotsTimingChanged(previousContext.Value.sessionId, previousContext.Value.trackId, previousPilotId.Value));
        }

        var newPilotLastDetection = _timingProjection.Object.GetLastPilotDetection(currentContext.Value.sessionId, currentContext.Value.trackId, newPilotId.Value);
        if (newPilotLastDetection?.Id == detection.Id)
        {
            await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(currentContext.Value.sessionId, currentContext.Value.trackId, detection.Id, newPilotId.Value, currentDetection.UtcTime));
        }

        await _eventChannel.PublishAsync(new PilotsTimingChanged(currentContext.Value.sessionId, currentContext.Value.trackId, newPilotId.Value));
    }

    private async Task When(DetectionSessionOverridden detection)
    {
        var previousTimingProjection = _timingProjection;
        var previousDetection = previousTimingProjection?.Object.GetDetection(detection.Id);

        _timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var currentDetection = _timingProjection.Object.GetDetection(detection.Id);
        if (currentDetection is null || !currentDetection.PilotId.HasValue)
        {
            return;
        }

        var pilotId = currentDetection.PilotId.Value;
        var previousContext = await ResolveSessionAndTrack(previousDetection?.SessionId, previousDetection?.TrackId);
        var currentContext = await ResolveSessionAndTrack(currentDetection.SessionId, currentDetection.TrackId);

        if (!currentContext.HasValue)
        {
            return;
        }

        if (previousContext.HasValue && previousContext.Value.sessionId == currentContext.Value.sessionId && previousContext.Value.trackId == currentContext.Value.trackId)
        {
            return;
        }

        if (previousContext.HasValue)
        {
            var previousLastDetection = previousTimingProjection?.Object.GetLastPilotDetection(previousContext.Value.sessionId, previousContext.Value.trackId, pilotId);
            if (previousLastDetection?.Id == detection.Id)
            {
                var oldSessionNewLastDetection = _timingProjection.Object.GetLastPilotDetection(previousContext.Value.sessionId, previousContext.Value.trackId, pilotId);
                if (oldSessionNewLastDetection is null)
                {
                    await _eventChannel.PublishAsync(new PilotsMostRecentDetectionCleared(previousContext.Value.sessionId, previousContext.Value.trackId, pilotId));
                }
                else
                {
                    await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(previousContext.Value.sessionId, previousContext.Value.trackId, oldSessionNewLastDetection.Id, pilotId, oldSessionNewLastDetection.UtcTime));
                }
            }

            await _eventChannel.PublishAsync(new PilotsTimingChanged(previousContext.Value.sessionId, previousContext.Value.trackId, pilotId));
        }

        var newSessionLastDetection = _timingProjection.Object.GetLastPilotDetection(currentContext.Value.sessionId, currentContext.Value.trackId, pilotId);
        if (newSessionLastDetection?.Id == detection.Id)
        {
            await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(currentContext.Value.sessionId, currentContext.Value.trackId, detection.Id, pilotId, currentDetection.UtcTime));
        }

        await _eventChannel.PublishAsync(new PilotsTimingChanged(currentContext.Value.sessionId, currentContext.Value.trackId, pilotId));
    }

    private async Task When(DetectionStatusSet detection)
    {
        var previousTimingProjection = _timingProjection;
        var previousDetection = previousTimingProjection?.Object.GetDetection(detection.Id);

        _timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var currentDetection = _timingProjection.Object.GetDetection(detection.Id);
        if (previousDetection is null || currentDetection is null || previousDetection.IsValid == currentDetection.IsValid || !currentDetection.PilotId.HasValue)
        {
            return;
        }

        var context = await ResolveSessionAndTrack(currentDetection.SessionId, currentDetection.TrackId);
        if (!context.HasValue)
        {
            return;
        }

        await _eventChannel.PublishAsync(new PilotsTimingChanged(context.Value.sessionId, context.Value.trackId, currentDetection.PilotId.Value));
    }

    private async Task When(DetectionMoved detection)
    {
        var previousTimingProjection = _timingProjection;
        var previousDetection = previousTimingProjection?.Object.GetDetection(detection.Id);

        _timingProjection = await _projectionProvider.CloneOrPull(_timingProjection);

        var currentDetection = _timingProjection.Object.GetDetection(detection.Id);

        if (currentDetection is null || !currentDetection.SessionId.HasValue || !currentDetection.PilotId.HasValue)
        {
            return;
        }

        var context = await ResolveSessionAndTrack(currentDetection.SessionId, currentDetection.TrackId);

        if (!context.HasValue)
        {
            return;
        }

        var currentLastDetection = _timingProjection.Object.GetLastPilotDetection(context.Value.sessionId, context.Value.trackId, currentDetection.PilotId.Value);
        var previousLastDetection = previousTimingProjection?.Object.GetLastPilotDetection(context.Value.sessionId, context.Value.trackId, currentDetection.PilotId.Value);

        if (currentLastDetection?.Id == detection.Id)
        {
            await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(context.Value.sessionId, context.Value.trackId, detection.Id, currentDetection.PilotId.Value, detection.UtcTime));
        }
        else if (previousLastDetection?.Id == detection.Id)
        {
            if (currentLastDetection is null)
            {
                await _eventChannel.PublishAsync(new PilotsMostRecentDetectionCleared(context.Value.sessionId, context.Value.trackId, currentDetection.PilotId.Value));
            }
            else
            {
                await _eventChannel.PublishAsync(new PilotsMostRecentDetectionChanged(context.Value.sessionId, context.Value.trackId, currentLastDetection.Id, currentDetection.PilotId.Value, currentLastDetection.UtcTime));
            }
        }

        await _eventChannel.PublishAsync(new PilotsTimingChanged(context.Value.sessionId, context.Value.trackId, currentDetection.PilotId.Value));
    }

    private async Task<(Guid sessionId, Guid trackId)?> ResolveSessionAndTrack(Guid? sessionId, Guid? trackId)
    {
        if (!sessionId.HasValue)
        {
            return null;
        }

        if (trackId.HasValue)
        {
            return (sessionId.Value, trackId.Value);
        }

        _sessionProjection = await _projectionProvider.CloneOrPull(_sessionProjection);

        var sessionInfo = _sessionProjection.Object.GetSessionById(sessionId.Value);
        if (sessionInfo is null)
        {
            return null;
        }

        return (sessionInfo.Id, sessionInfo.TrackId);
    }
}
