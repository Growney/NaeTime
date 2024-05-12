using NaeTime.Hardware.Messages.Messages;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Hardware.ImmersionRC;
public class EthernetLapRF8ChannelTimerLaneService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public EthernetLapRF8ChannelTimerLaneService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(TimerConnectionEstablished connectionEstablished)
    {
        var laneConfigurations = await _rpcClient.InvokeAsync<IEnumerable<Timing.Messages.Models.ActiveLaneConfiguration>>("GetActiveLaneConfigurations");

        var timerDetails = await _rpcClient.InvokeAsync<Messages.Models.TimerDetails>("GetTimerDetails", connectionEstablished.TimerId);

        if (timerDetails == null || timerDetails.Type != Messages.Models.TimerType.EthernetLapRF8Channel)
        {
            return;
        }

        var timerLaneConfigurationReponse = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration>>("GetEthernetLapRF8ChannelTimerLaneConfigurations", connectionEstablished.TimerId);

        //We have no lane configurations update the configurations with those from the timer
        if (laneConfigurations == null || !laneConfigurations.Any())
        {
            if (timerLaneConfigurationReponse != null && timerLaneConfigurationReponse.Any())
            {
                foreach (var timerLane in timerLaneConfigurationReponse)
                {
                    await GenerateConfigurationEvents(timerLane).ConfigureAwait(false);
                }
            }
        }
        //We have some configuration configuration
        else
        {
            if (timerLaneConfigurationReponse != null)
            {
                //Loop through the lane configuration and update the timer configuration
                await ReconfigureTimerLanes(connectionEstablished, laneConfigurations, timerLaneConfigurationReponse).ConfigureAwait(false);

                await ReconfigurationLocalFromTimer(laneConfigurations, timerLaneConfigurationReponse).ConfigureAwait(false);
            }
        }
    }

    private async Task ReconfigurationLocalFromTimer(IEnumerable<Timing.Messages.Models.ActiveLaneConfiguration> laneConfigurations, IEnumerable<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration> timerLaneConfigurationReponse)
    {
        foreach (var timerLane in timerLaneConfigurationReponse)
        {
            //The lane is not configured locally
            if (!laneConfigurations.Any(x => x.Lane == timerLane.Lane))
            {
                await GenerateConfigurationEvents(timerLane).ConfigureAwait(false);
            }
        }
    }
    private async Task ReconfigureTimerLanes(TimerConnectionEstablished connectionEstablished, IEnumerable<Timing.Messages.Models.ActiveLaneConfiguration> laneConfigurations, IEnumerable<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration> timerLaneConfigurationReponse)
    {
        var timerReconfigurations = new List<TimersLaneConfigured.LaneConfiguration>();
        foreach (var lane in laneConfigurations)
        {
            var timerLane = timerLaneConfigurationReponse.FirstOrDefault(x => x.Lane == lane.Lane);
            if (timerLane != null)
            {
                bool shouldChange =
                    timerLane.IsEnabled != lane.IsEnabled || timerLane.FrequencyInMhz != lane.FrequencyInMhz;

                if (shouldChange)
                {
                    timerReconfigurations.Add(new TimersLaneConfigured.LaneConfiguration(lane.Lane, timerLane.FrequencyInMhz, timerLane.IsEnabled));
                }
            }
        }

        if (timerReconfigurations.Any())
        {
            await _eventClient.Publish(new TimersLaneConfigured(connectionEstablished.TimerId, timerReconfigurations)).ConfigureAwait(false);
        }
    }
    public async Task GenerateConfigurationEvents(Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration configuration)
    {
        if (configuration.IsEnabled)
        {
            await _eventClient.Publish(new LaneEnabled(configuration.Lane)).ConfigureAwait(false);
        }
        else
        {
            await _eventClient.Publish(new LaneDisabled(configuration.Lane)).ConfigureAwait(false);
        }

        if (configuration.FrequencyInMhz != null)
        {
            await _eventClient.Publish(new LaneRadioFrequencyConfigured(configuration.Lane, null, configuration.FrequencyInMhz.Value)).ConfigureAwait(false);
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        var timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.EthernetLapRF8ChannelTimer>>("GetAllEthernetLapRF8ChannelTimers");

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers)
        {
            var timerLaneConfiguration = await _rpcClient.InvokeAsync<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration?>("GetEthernetLapRF8ChannelTimerLaneConfiguration", timer.TimerId, laneEnabled.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || !timerLaneConfiguration.IsEnabled)
            {
                await _eventClient.Publish(new EthernetLapRF8ChannelTimerLaneEnabled(timer.TimerId, laneEnabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.EthernetLapRF8ChannelTimer>>("GetAllEthernetLapRF8ChannelTimers");

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers)
        {
            var timerLaneConfiguration = await _rpcClient.InvokeAsync<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration?>("GetEthernetLapRF8ChannelTimerLaneConfiguration", timer.TimerId, laneDisabled.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.IsEnabled)
            {
                await _eventClient.Publish(new EthernetLapRF8ChannelTimerLaneDisabled(timer.TimerId, laneDisabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        var timers = await _rpcClient.InvokeAsync<IEnumerable<Messages.Models.EthernetLapRF8ChannelTimer>>("GetAllEthernetLapRF8ChannelTimers");

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers)
        {
            var timerLaneConfiguration = await _rpcClient.InvokeAsync<Messages.Models.EthernetLapRF8ChannelTimerLaneConfiguration?>("GetEthernetLapRF8ChannelTimerLaneConfiguration", timer.TimerId, frequencyChange.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.FrequencyInMhz != frequencyChange.FrequencyInMhz)
            {
                await _eventClient.Publish(new EthernetLapRF8ChannelTimerLaneRadioFrequencyConfigured(timer.TimerId, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz)).ConfigureAwait(false);
            }
        }
    }
}
