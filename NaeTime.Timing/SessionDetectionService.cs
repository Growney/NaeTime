using NaeTime.Hardware.Messages;
using NaeTime.Management.Messages.Models;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing;
internal class SessionDetectionService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public SessionDetectionService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public Task When(TimerDetectionTriggered triggered) => TriggerTrackDetection(triggered.TimerId, triggered.Lane, null, triggered.SoftwareTime, triggered.UtcTime);

    public Task When(TimerDetectionOccured occured) => TriggerTrackDetection(occured.TimerId, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime);

    private async Task TriggerTrackDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        ActiveSession? activeSessionResponse = await _rpcClient.InvokeAsync<ActiveSession?>("GetActiveSession");

        if (activeSessionResponse == null)
        {
            return;
        }

        if (activeSessionResponse.Type == ActiveSession.SessionType.OpenPractice)
        {
            await _eventClient.PublishAsync(new ActiveOpenPracticeSessionDetectionOccured(activeSessionResponse.SessionId, timerId, lane, hardwareTime, softwareTime, utcTime));
        }
    }
}
