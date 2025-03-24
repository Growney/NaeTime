using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.OpenPractice;
public class OpenPracticeSessionManager
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;

    public OpenPracticeSessionManager(IEventClient eventClient, INaeTimePersistence persistence)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public async Task When(LapCompleted lapCompleted)
    {
        Persistence.Abstractions.OpenPractice.OpenPracticeSession? sessionResponse = await _persistence.OpenPractice.GetOpenPracticeSession(lapCompleted.SessionId);

        if (sessionResponse == null)
        {
            return;
        }

        Persistence.Abstractions.OpenPractice.PilotLane? pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapCompleted.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _eventClient.PublishAsync(new OpenPracticeLapCompleted(Guid.NewGuid(), lapCompleted.SessionId, pilotLane.PilotId, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, lapCompleted.TotalTime)).ConfigureAwait(false);
    }
    public async Task When(LapInvalidated lapInvalidated)
    {
        Persistence.Abstractions.OpenPractice.OpenPracticeSession? sessionResponse = await _persistence.OpenPractice.GetOpenPracticeSession(lapInvalidated.SessionId);

        if (sessionResponse == null)
        {
            return;
        }

        Persistence.Abstractions.OpenPractice.PilotLane? pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapInvalidated.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _eventClient.PublishAsync(new OpenPracticeLapInvalidated(Guid.NewGuid(), lapInvalidated.SessionId, pilotLane.PilotId, lapInvalidated.LapNumber, lapInvalidated.StartedUtcTime, lapInvalidated.FinishedUtcTime, lapInvalidated.TotalTime));
    }
}
