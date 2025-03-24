using NaeTime.Hardware.Messages;
using NaeTime.OpenPractice.Messages.Events;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing;
internal class SessionDetectionService
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;

    public SessionDetectionService(IEventClient eventClient, INaeTimePersistence persistence)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public Task When(TimerDetectionTriggered triggered) => TriggerTrackDetection(triggered.TimerId, triggered.Lane, null, triggered.SoftwareTime, triggered.UtcTime);

    public Task When(TimerDetectionOccured occured) => TriggerTrackDetection(occured.TimerId, occured.Lane, occured.HardwareTime, occured.SoftwareTime, occured.UtcTime);

    private async Task TriggerTrackDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        Persistence.Abstractions.Management.ActiveSession? activeSessionResponse = await _persistence.Management.GetActiveSession();

        if (activeSessionResponse == null)
        {
            return;
        }

        if (activeSessionResponse.Type == Persistence.Abstractions.Management.ActiveSession.SessionType.OpenPractice)
        {
            await _eventClient.PublishAsync(new ActiveOpenPracticeSessionDetectionOccured(activeSessionResponse.SessionId, timerId, lane, hardwareTime, softwareTime, utcTime));
        }
    }
}
