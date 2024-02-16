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

    public async Task When(SessionDetectionTriggered triggered)
    {
        var session = await _dispatcher.Request<SessionRequest, SessionResponse>(new SessionRequest(triggered.SessionId)).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var track = await _dispatcher.Request<TrackRequest, TrackResponse>(new TrackRequest(session.TrackId)).ConfigureAwait(false);

        if (track == null)
        {
            return;
        }

        var timerCount = track.Timers.Count();

        if (timerCount > byte.MaxValue)
        {
            return;
        }


        await _dispatcher.Dispatch(new SessionDetectionOccured(session.Id, triggered.Lane, triggered.Split, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, (byte)timerCount, null, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
    }

    public async Task When(SessionInvalidationTriggered triggered)
    {
        var session = await _dispatcher.Request<SessionRequest, SessionResponse>(new SessionRequest(triggered.SessionId)).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        var activeTimings = await _dispatcher.Request<ActiveTimingRequest, ActiveTimingResponse>(new ActiveTimingRequest(triggered.SessionId, triggered.Lane)).ConfigureAwait(false);

        if (activeTimings == null)
        {
            return;
        }

        if (activeTimings.Lap == null)
        {
            return;
        }

        var finishedSoftwareTime = _softwareTimer.ElapsedMilliseconds;
        var finishedUtcTime = DateTime.UtcNow;

        var totalTime = CalculateTotalTime(activeTimings.Lap.StartedSoftwareTime, activeTimings.Lap.StartedUtcTime, finishedSoftwareTime, finishedUtcTime);

        await _dispatcher.Dispatch(new LapInvalidated(session.Id, triggered.Lane, activeTimings.LapNumber, activeTimings.Lap.StartedSoftwareTime, activeTimings.Lap.StartedUtcTime, activeTimings.Lap.StartedHardwareTime, finishedSoftwareTime, finishedUtcTime, null, totalTime, LapInvalidated.LapInvalidReason.Cancelled)).ConfigureAwait(false);
    }
    private long CalculateTotalTime(long startSoftwareTime, DateTime startUtcTime, long endSoftwareTime, DateTime endUtcTime)
    {
        var softwareDifference = endSoftwareTime - startSoftwareTime;
        if (softwareDifference < 0)
        {
            return (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds;
        }
        else
        {
            return softwareDifference;
        }
    }
}
