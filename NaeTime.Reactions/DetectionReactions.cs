using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Query.Abstractions;

namespace NaeTime.Reactions;
internal class DetectionReactions
{
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly IDetectionCommandHandler _detectionCommandHandler;
    public DetectionReactions(ISessionQueryHandler sessionQueryHandler, IDetectionCommandHandler detectionCommandHandler)
    {
        _sessionQueryHandler = sessionQueryHandler ?? throw new ArgumentNullException(nameof(sessionQueryHandler));
        _detectionCommandHandler = detectionCommandHandler ?? throw new ArgumentNullException(nameof(detectionCommandHandler));
    }

    public async Task When(HardwareDetectionOccured detection)
    {
        NaeTime.Query.Abstractions.Models.Session? session = await _sessionQueryHandler.GetActiveSession();

        if (session is null)
        {
            return;
        }

        switch (session.Type)
        {
            case Query.Abstractions.Models.SessionType.OpenPractice:
                await _detectionCommandHandler.BindDetectionToOpenPracticeSession(detection.DetectionId, session.Id);
                break;
            default:
                break;
        }
    }

    public async Task When(DetectionTriggered detection)
    {
        NaeTime.Query.Abstractions.Models.Session? session = await _sessionQueryHandler.GetActiveSession();

        if (session is null)
        {
            return;
        }

        switch (session.Type)
        {
            case Query.Abstractions.Models.SessionType.OpenPractice:
                await _detectionCommandHandler.BindDetectionToOpenPracticeSession(detection.DetectionId, session.Id);
                break;
            default:
                break;
        }
    }
}
