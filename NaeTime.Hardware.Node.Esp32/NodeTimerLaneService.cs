using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeTimerLaneService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public NodeTimerLaneService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        IEnumerable<Messages.Models.SerialEsp32Node>? timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.SerialEsp32Node>>("GetAllSerialEsp32NodeTimers");

        if (timers == null)
        {
            return;
        }

        foreach (Messages.Models.SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneRadioFrequencyConfigured(timer.TimerId, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz)).ConfigureAwait(false);
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        IEnumerable<Messages.Models.SerialEsp32Node>? timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.SerialEsp32Node>>("GetAllSerialEsp32NodeTimers");
        if (timers == null)
        {
            return;
        }

        foreach (Messages.Models.SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneDisabled(timer.TimerId, laneDisabled.LaneNumber)).ConfigureAwait(false);
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        IEnumerable<Messages.Models.SerialEsp32Node>? timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.SerialEsp32Node>>("GetAllSerialEsp32NodeTimers");
        if (timers == null)
        {
            return;
        }

        foreach (Messages.Models.SerialEsp32Node timer in timers)
        {
            await _eventClient.PublishAsync(new NodeTimerLaneEnabled(timer.TimerId, laneEnabled.LaneNumber)).ConfigureAwait(false);
        }
    }
}
