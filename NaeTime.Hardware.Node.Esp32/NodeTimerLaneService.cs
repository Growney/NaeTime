using NaeTime.Hardware.Messages;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeTimerLaneService
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;

    public NodeTimerLaneService(IEventClient eventClient, INaeTimePersistence persistence)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
    }

    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        IEnumerable<SerialEsp32Node> timers = await _persistence.Hardware.GetAllSerialEsp32NodeTimers();

        if (timers == null)
        {
            return;
        }

        foreach (SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneRadioFrequencyConfigured(timer.TimerId, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz)).ConfigureAwait(false);
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        IEnumerable<SerialEsp32Node> timers = await _persistence.Hardware.GetAllSerialEsp32NodeTimers();

        foreach (SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneDisabled(timer.TimerId, laneDisabled.LaneNumber)).ConfigureAwait(false);
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        IEnumerable<SerialEsp32Node> timers = await _persistence.Hardware.GetAllSerialEsp32NodeTimers();

        foreach (SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneEnabled(timer.TimerId, laneEnabled.LaneNumber)).ConfigureAwait(false);
        }
    }
}
