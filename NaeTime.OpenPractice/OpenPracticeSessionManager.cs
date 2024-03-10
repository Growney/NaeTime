using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.OpenPractice;
public class OpenPracticeSessionManager : ISubscriber
{
    private readonly IDispatcher _dispatch;

    public OpenPracticeSessionManager(IPublishSubscribe publishSubscribe)
    {
        _dispatch = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public async Task When(LapCompleted lapCompleted)
    {
        var sessionResponse = await _dispatch.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(lapCompleted.SessionId)).ConfigureAwait(false);

        if (sessionResponse == null)
        {
            return;
        }

        var pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapCompleted.Lane);
        if (pilotLane == null)
        {
            return;
        }

        var newLapId = Guid.NewGuid();

        await _dispatch.Dispatch(new OpenPracticeLapCompleted(Guid.NewGuid(), lapCompleted.SessionId, pilotLane.PilotId, lapCompleted.StartedUtcTime, lapCompleted.FinishedUtcTime, lapCompleted.TotalTime)).ConfigureAwait(false);
    }
    public async Task When(LapInvalidated lapInvalidated)
    {
        var sessionResponse = await _dispatch.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(lapInvalidated.SessionId));

        if (sessionResponse == null)
        {
            return;
        }

        var pilotLane = sessionResponse.ActiveLanes.FirstOrDefault(x => x.Lane == lapInvalidated.Lane);
        if (pilotLane == null)
        {
            return;
        }

        await _dispatch.Dispatch(new OpenPracticeLapInvalidated(Guid.NewGuid(), lapInvalidated.SessionId, pilotLane.PilotId, lapInvalidated.LapNumber, lapInvalidated.StartedUtcTime, lapInvalidated.FinishedUtcTime, lapInvalidated.TotalTime));
    }
}
