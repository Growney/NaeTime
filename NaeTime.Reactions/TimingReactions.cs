using NaeTime.Command.Aggregates;
using NaeTime.Events.Domain;
using NaeTime.Events.Integration;
using NaeTime.Query.Abstractions;
using NaeTime.Reactions.Abstractions;

namespace NaeTime.Reactions;

public class TimingReactions
{
    private readonly ISessionQueryHandler _sessionQueryHandler;
    private readonly IOpenPracticeQueryHandler _openPracticeQueryHandler;
    private readonly IEventChannel _eventChannel;

    public TimingReactions(ISessionQueryHandler sessionQueryHandler, IOpenPracticeQueryHandler openPracticeQueryHandler, IEventChannel eventChannel)
    {
        _sessionQueryHandler = sessionQueryHandler;
        _openPracticeQueryHandler = openPracticeQueryHandler;
        _eventChannel = eventChannel;
    }

    private async Task When(HardwareDetectionOccured detection)
    {
        var activeSession = await _sessionQueryHandler.GetActiveSession();

        if (activeSession == null)
        {
            return;
        }

        await _eventChannel.PublishAsync(new SessionTimingChanged(activeSession.Id, detection.Id));
    }
}
