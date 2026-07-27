using NaeTime.Command.Aggregates;
using NaeTime.Events.Domain;
using NaeTime.Events.Integration;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Reactions.Abstractions;

namespace NaeTime.Reactions;

public class TimingReactions
{
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly ITimingQueryHandler _timingQueryHandler;
    private readonly IEventChannel _eventChannel;

    public TimingReactions(ISessionQueryHandler sessionQueryHandler, ITimingQueryHandler timingQueryHandler, IEventChannel eventChannel)
    {
        _sessionQueryHandler = sessionQueryHandler;
        _timingQueryHandler = timingQueryHandler;
        _eventChannel = eventChannel;
    }

    private Task When(HardwareDetectionOccured detection) => HandleDetection(detection.Id, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
    private Task When(DetectionManuallyTriggered detection) => HandleDetection(detection.Id, detection.Lane, null, detection.SoftwareTime, detection.UtcTime);
    private async Task HandleDetection(Guid detectionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var activeSession = await _sessionQueryHandler.GetActiveSession();

        if (activeSession == null)
        {
            await _eventChannel.PublishAsync(new LiveDetectionOccurredWithNoActiveSession(detectionId));
            return;
        }

        await HandleSessionDetection(activeSession.Id, detectionId, lane, hardwareTime, softwareTime, utcTime);
    }
    private async Task HandleSessionDetection(Guid sessionId, Guid detectionId,byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        await _eventChannel.PublishAsync(new LiveDetectionAssignedToSession(sessionId, detectionId));

        var lanePilot = await _timingQueryHandler.GetLanePilot(sessionId, lane);

        if (!lanePilot.HasValue)
        {
            return;
        }

        await _eventChannel.PublishAsync(new LiveDetectionAssignedToPilot(sessionId, detectionId, lanePilot.Value));
    }
}
