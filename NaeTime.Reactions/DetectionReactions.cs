using EventDbLite.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;

namespace NaeTime.Reactions;
internal class DetectionReactions
{
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly IOpenPracticeQueryHandler _openPracticeQueryHandler;
    private readonly ITrackQueryHandler _trackQueryHandler;
    private readonly IStreamEventWriter _eventWriter;

    public DetectionReactions(ISessionQueryHandler sessionQueryHandler, IOpenPracticeQueryHandler openPracticeQueryHandler, ITrackQueryHandler trackQueryHandler, IStreamEventWriter eventWriter)
    {
        _sessionQueryHandler = sessionQueryHandler ?? throw new ArgumentNullException(nameof(sessionQueryHandler));
        _openPracticeQueryHandler = openPracticeQueryHandler ?? throw new ArgumentNullException(nameof(openPracticeQueryHandler));
        _trackQueryHandler = trackQueryHandler ?? throw new ArgumentNullException(nameof(trackQueryHandler));
        _eventWriter = eventWriter ?? throw new ArgumentNullException(nameof(eventWriter));
    }

    public async Task When(HardwareDetectionOccured detection)
    {
        NaeTime.Query.Abstractions.Models.Session? session = await _sessionQueryHandler.GetActiveSession();

        if (session is null)
        {
            return;
        }

        Task processTask = session.Type switch
        {
            Query.Abstractions.Models.SessionType.OpenPractice => HandleDetectionDuringOpenPracticeSession(detection, session.Id),
            _ => Task.CompletedTask
        };

        await processTask;
    }

    private async Task HandleDetectionDuringOpenPracticeSession(HardwareDetectionOccured detection, Guid sessionId)
    {
        Query.Abstractions.Models.OpenPracticeSession? session = await _openPracticeQueryHandler.GetByIdAsync(sessionId);

        if (session is null)
        {
            return;
        }

        Query.Abstractions.Models.Track? track = await _trackQueryHandler.GetTrack(session.TrackId);
        if (track is null)
        {
            return;
        }

        byte detectorIndex = (byte)Array.FindIndex(track.Detectors, x => x.Id == detection.TimerId);

        if (detectorIndex < 0)
        {
            return;
        }

        string streamName = $"OpenPracticeSession-Timing-{sessionId:N}";

        await _eventWriter.AppendToStream(streamName, new OpenPracticeHardwareDetectionOccured(detection.Id, sessionId, detection.TimerId, detectorIndex, (byte)track.Detectors.Length, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));

        Query.Abstractions.Models.OpenPracticeLane? laneInfo = session.Lanes.FirstOrDefault(l => l.Lane == detection.Lane);

        if (laneInfo is null)
        {
            return;
        }

        if (!laneInfo.IsEnabled)
        {
            await _eventWriter.AppendToStream(streamName, new OpenPracticeHardwareDetectionIgnoredOnDisabledLane(detection.Id, sessionId, detection.TimerId, detectorIndex, (byte)track.Detectors.Length, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
            return;
        }

        if (laneInfo.PilotId is null)
        {
            await _eventWriter.AppendToStream(streamName, new OpenPracticeHardwareDetectionIgnoredOnUnassignedLane(detection.Id, sessionId, detection.TimerId, detectorIndex, (byte)track.Detectors.Length, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
            return;
        }

        string pilotTimingStreamName = $"OpenPracticePilotTiming-{sessionId:N}-{laneInfo.PilotId:N}";

        await _eventWriter.AppendToStream(pilotTimingStreamName, new OpenPracticePilotDetectionOccured(detection.Id, sessionId, laneInfo.PilotId.Value, detection.TimerId, detectorIndex, (byte)track.Detectors.Length, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }
}
