using NaeTime.Hardware.Messages.Messages;
using NaeTime.Management.Messages.Requests;
using NaeTime.Management.Messages.Responses;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing;
internal class SessionDetectionService : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public SessionDetectionService(IPublishSubscribe dispatcher)
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

        if (activeSessionResponse.Type == ActiveSessionResponse.SessionType.OpenPractice)
        {
            await _publishSubscribe.Dispatch(new ActiveOpenPracticeSessionDetectionOccured(activeSessionResponse.SessionId, timerId, lane, hardwareTime, softwareTime, utcTime));
        }
    }
}
