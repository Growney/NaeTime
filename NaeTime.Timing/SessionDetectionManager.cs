using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class SessionDetectionManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public SessionDetectionManager(IPublishSubscribe dispatcher)
    {
        _publishSubscribe = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
    }

    public Task When(TimerDetectionTriggered triggered) => TriggerTrackDetection(triggered.TimerId, triggered.Lane, null, triggered.SoftwareTime, triggered.UtcTime);

    public Task When(TimerDetectionOccured occured) => TriggerTrackDetection(occured.TimerId, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime);

    private async Task TriggerTrackDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var activeSessionResponse = await _publishSubscribe.Request<ActiveSessionRequest, ActiveSessionResponse>().ConfigureAwait(false);

        if (activeSessionResponse == null)
        {
            return;
        }

        ActiveTrack activeTrack = new(activeSessionResponse.ActiveTrack.Timers);

        var timerPosition = activeTrack.GetTimerPosition(timerId);
        var timerCount = activeTrack.Timers.Count();
        if (timerPosition < 0 || timerPosition > byte.MaxValue || timerCount > byte.MaxValue)
        {
            //TODO dispatch timer detection discarded
            return;
        }

        await _publishSubscribe.Dispatch(new SessionDetectionOccured(activeSessionResponse.SessionId, lane, (byte)timerPosition, activeSessionResponse.MinimumLapMilliseconds, activeSessionResponse.MaximumLapMilliseconds, (byte)timerCount, hardwareTime, softwareTime, utcTime)).ConfigureAwait(false);
    }
}
