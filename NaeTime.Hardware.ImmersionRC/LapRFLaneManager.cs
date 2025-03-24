using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;
using NaeTime.Hardware.Messages;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;

namespace NaeTime.Hardware.ImmersionRC;
public class LapRFLaneManager
{
    private readonly IEventClient _eventClient;
    private readonly INaeTimePersistence _persistence;
    private readonly ILapRFManager _lapRFManager;

    public LapRFLaneManager(IEventClient eventClient, INaeTimePersistence persistence, ILapRFManager lapRFManager)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _lapRFManager = lapRFManager ?? throw new ArgumentNullException(nameof(lapRFManager));
    }

    public async Task When(TimerConnectionEstablished connectionEstablished)
    {
        IEnumerable<ActiveLaneConfiguration>? laneConfigurations = await _persistence.Timing.GetActiveLaneConfigurations();

        Persistence.Abstractions.Hardware.TimerType timerDetails = await _persistence.Hardware.GetTimerType(connectionEstablished.TimerId);

        if (timerDetails != Persistence.Abstractions.Hardware.TimerType.EthernetLapRF8Channel)
        {
            return;
        }

        IEnumerable<LapRFLaneConfiguration>? timerLaneConfigurationReponse = await _lapRFManager.GetTimeLaneConfigurations(connectionEstablished.TimerId);

        //We have no lane configurations update the configurations with those from the timer
        if (laneConfigurations == null || !laneConfigurations.Any())
        {
            if (timerLaneConfigurationReponse != null && timerLaneConfigurationReponse.Any())
            {
                foreach (LapRFLaneConfiguration timerLane in timerLaneConfigurationReponse)
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

    private async Task ReconfigurationLocalFromTimer(IEnumerable<ActiveLaneConfiguration> laneConfigurations, IEnumerable<LapRFLaneConfiguration> timerLaneConfigurationReponse)
    {
        foreach (LapRFLaneConfiguration timerLane in timerLaneConfigurationReponse)
        {
            //The lane is not configured locally
            if (!laneConfigurations.Any(x => x.Lane == timerLane.Lane))
            {
                await GenerateConfigurationEvents(timerLane).ConfigureAwait(false);
            }
        }
    }
    private async Task ReconfigureTimerLanes(TimerConnectionEstablished connectionEstablished, IEnumerable<ActiveLaneConfiguration> laneConfigurations, IEnumerable<LapRFLaneConfiguration> timerLaneConfigurationReponse)
    {
        List<TimersLaneConfigured.LaneConfiguration> timerReconfigurations = new();
        foreach (ActiveLaneConfiguration lane in laneConfigurations)
        {
            LapRFLaneConfiguration? timerLane = timerLaneConfigurationReponse.FirstOrDefault(x => x.Lane == lane.Lane);
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
            await _eventClient.PublishAsync(new TimersLaneConfigured(connectionEstablished.TimerId, timerReconfigurations)).ConfigureAwait(false);
        }
    }
    public async Task GenerateConfigurationEvents(LapRFLaneConfiguration configuration)
    {
        if (configuration.IsEnabled)
        {
            await _eventClient.PublishAsync(new LaneEnabled(configuration.Lane)).ConfigureAwait(false);
        }
        else
        {
            await _eventClient.PublishAsync(new LaneDisabled(configuration.Lane)).ConfigureAwait(false);
        }

        if (configuration.FrequencyInMhz != null)
        {
            await _eventClient.PublishAsync(new LaneRadioFrequencyConfigured(configuration.Lane, null, configuration.FrequencyInMhz.Value)).ConfigureAwait(false);
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        IEnumerable<EthernetLapRF8ChannelTimer> timers = await _persistence.Hardware.GetAllEthernetLapRF8ChannelTimers();

        foreach (EthernetLapRF8ChannelTimer timer in timers)
        {
            LapRFLaneConfiguration? timerLaneConfiguration = await _lapRFManager.GetTimerLaneConfiguration(timer.TimerId, laneEnabled.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || !timerLaneConfiguration.IsEnabled)
            {
                await _eventClient.PublishAsync(new EthernetLapRF8ChannelTimerLaneEnabled(timer.TimerId, laneEnabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        IEnumerable<EthernetLapRF8ChannelTimer> timers = await _persistence.Hardware.GetAllEthernetLapRF8ChannelTimers();

        foreach (EthernetLapRF8ChannelTimer timer in timers)
        {
            LapRFLaneConfiguration? timerLaneConfiguration = await _lapRFManager.GetTimerLaneConfiguration(timer.TimerId, laneDisabled.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.IsEnabled)
            {
                await _eventClient.PublishAsync(new EthernetLapRF8ChannelTimerLaneDisabled(timer.TimerId, laneDisabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        IEnumerable<EthernetLapRF8ChannelTimer> timers = await _persistence.Hardware.GetAllEthernetLapRF8ChannelTimers();

        foreach (EthernetLapRF8ChannelTimer timer in timers)
        {
            LapRFLaneConfiguration? timerLaneConfiguration = await _lapRFManager.GetTimerLaneConfiguration(timer.TimerId, frequencyChange.LaneNumber).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.FrequencyInMhz != frequencyChange.FrequencyInMhz)
            {
                await _eventClient.PublishAsync(new EthernetLapRF8ChannelTimerLaneRadioFrequencyConfigured(timer.TimerId, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz)).ConfigureAwait(false);
            }
        }
    }
}
