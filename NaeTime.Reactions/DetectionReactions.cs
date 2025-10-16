using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;

namespace NaeTime.Reactions;
internal class DetectionReactions
{
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly IOpenPracticeCommandHandler _openPracticeCommandHandler;
    private readonly IStreamEventWriter _streamEventWriter;

    public DetectionReactions(ISessionQueryHandler sessionQueryHandler, IOpenPracticeCommandHandler openPracticeCommandHandler, IStreamEventWriter streamEventWriter)
    {
        _sessionQueryHandler = sessionQueryHandler ?? throw new ArgumentNullException(nameof(sessionQueryHandler));
        _openPracticeCommandHandler = openPracticeCommandHandler ?? throw new ArgumentNullException(nameof(openPracticeCommandHandler));
        _streamEventWriter = streamEventWriter ?? throw new ArgumentNullException(nameof(streamEventWriter));
    }
    public async Task When(HardwareDetectionOccured detection)
    {
        NaeTime.Query.Abstractions.Models.Session? session = await _sessionQueryHandler.GetActiveSession();

        if (session is null)
        {
            await _streamEventWriter.AppendToStream("unassigned-detections", new HardwareDetectionOccuredWithNoActiveSession(detection.Id, detection.TimerId, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
            return;
        }

        Task processTask = session.Type switch
        {
            Query.Abstractions.Models.SessionType.OpenPractice => HandleDetectionDuringOpenPracticeSession(detection, session.Id),
            _ => Task.CompletedTask
        };

        await processTask;
    }

    private async Task HandleDetectionDuringOpenPracticeSession(HardwareDetectionOccured detection, Guid sessionId) => await _openPracticeCommandHandler.AssignHardwareDetectionToSession(detection.Id, sessionId, detection.TimerId, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
}
