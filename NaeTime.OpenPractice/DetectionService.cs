using NaeTime.Hardware.Abstractions;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.OpenPractice.Messages.Requests;
using NaeTime.OpenPractice.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;
using NaeTime.Timing.Messages.Requests;
using NaeTime.Timing.Messages.Responses;

namespace NaeTime.OpenPractice;
internal class DetectionService : ISubscriber
{
    private readonly IDispatcher _dispatcher;
    private readonly ISoftwareTimer _softwareTimer;

    public DetectionService(IDispatcher dispatcher, ISoftwareTimer softwareTimer)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public async Task When(OpenPracticeSessionDetectionTriggered triggered)
    {
        var session = await _dispatcher.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(triggered.SessionId)).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await _dispatcher.Dispatch(new SessionDetectionOccured(session.Id, triggered.TimerId, triggered.Lane, session.TrackId, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, null, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
    }

    public async Task When(OpenPracticeSessionInvalidationTriggered triggered)
    {
        var session = await _dispatcher.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(triggered.SessionId)).ConfigureAwait(false);

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
    public async Task When(ActiveOpenPracticeSessionDetectionOccured detection)
    {
        var session = await _dispatcher.Request<OpenPracticeSessionRequest, OpenPracticeSessionResponse>(new OpenPracticeSessionRequest(detection.SessionId)).ConfigureAwait(false);

        if (session == null)
        {
            return;
        }

        await _dispatcher.Dispatch(new SessionDetectionOccured(session.Id, detection.TimerId, detection.Lane, session.TrackId, session.MinimumLapMilliseconds, session.MaximumLapMilliseconds, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime));
    }
}
