using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;

namespace NaeTime.Timing;
public class ManualDetectionManager : ISubscriber
{
    private readonly IDispatcher _dispatcher;
    private readonly ISoftwareTimer _softwareTimer;

    public ManualDetectionManager(IDispatcher dispatcher, ISoftwareTimer softwareTimer)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public async Task When(ActiveTrackSplitLaneDetectionTriggered triggered)
    {
        var activeSession = await _dispatcher.Request<ActiveSessionRequest, ActiveSessionResponse>();

        if (activeSession == null)
        {
            return;
        }

        var timerCount = activeSession.ActiveTrack.Timers.Count();

        if (timerCount > byte.MaxValue)
        {
            return;
        }

        await _dispatcher.Dispatch(new SessionDetectionOccured(activeSession.SessionId, triggered.Lane, triggered.Split, activeSession.MinimumLapMilliseconds, activeSession.MaximumLapMilliseconds, (byte)timerCount, null, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow));
    }
}
