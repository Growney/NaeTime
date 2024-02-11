using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing;
public class LaneTimerManager : ISubscriber
{
    private readonly IPublishSubscribe _publisher;

    public LaneTimerManager(IPublishSubscribe publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }
    public async Task When(TimerConnectionEstablished connectionEstablished)
    {
        var laneConfigurations = await _publisher.Request<ActiveLanesConfigurationRequest, ActiveLanesConfigurationResponse>();
        var timerLaneConfigurationReponse = await _publisher.Request<TimerLanesConfigurationRequest, TimerLanesConfigurationResponse>(new TimerLanesConfigurationRequest(connectionEstablished.TimerId));

        //We have no lane configurations update the configurations with those from the timer
        if (laneConfigurations == null || !laneConfigurations.Lanes.Any())
        {
            if (timerLaneConfigurationReponse != null && timerLaneConfigurationReponse.Lanes.Any())
            {
                foreach (var timerLane in timerLaneConfigurationReponse.Lanes)
                {
                    await GenerateConfigurationEvents(timerLane);
                }
            }
        }
        //We have some configuration configuration
        else
        {
            if (timerLaneConfigurationReponse != null)
            {
                //Loop through the lane configuration and update the timer configuration
                await ReconfigureTimerLanes(connectionEstablished, laneConfigurations, timerLaneConfigurationReponse);

                await ReconfigurationLocalFromTimer(laneConfigurations, timerLaneConfigurationReponse);
            }
        }
    }

    private async Task ReconfigurationLocalFromTimer(ActiveLanesConfigurationResponse laneConfigurations, TimerLanesConfigurationResponse timerLaneConfigurationReponse)
    {
        foreach (var timerLane in timerLaneConfigurationReponse.Lanes)
        {
            //The lane is not configured locally
            if (!laneConfigurations.Lanes.Any(x => x.Lane == timerLane.Lane))
            {
                await GenerateConfigurationEvents(timerLane);
            }
        }
    }
    private async Task ReconfigureTimerLanes(TimerConnectionEstablished connectionEstablished, ActiveLanesConfigurationResponse laneConfigurations, TimerLanesConfigurationResponse timerLaneConfigurationReponse)
    {
        var timerReconfigurations = new List<TimersLaneConfigured.LaneConfiguration>();
        foreach (var lane in laneConfigurations.Lanes)
        {
            var timerLane = timerLaneConfigurationReponse.Lanes.FirstOrDefault(x => x.Lane == lane.Lane);
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
            await _publisher.Dispatch(
                 new TimersLaneConfigured(connectionEstablished.TimerId, timerReconfigurations));
        }
    }
    public async Task GenerateConfigurationEvents(TimerLanesConfigurationResponse.TimerLaneConfiguration configuration)
    {
        if (configuration.IsEnabled)
        {
            await _publisher.Dispatch(new LaneEnabled(configuration.Lane));
        }
        else
        {
            await _publisher.Dispatch(new LaneDisabled(configuration.Lane));
        }

        if (configuration.FrequencyInMhz != null)
        {
            await _publisher.Dispatch(new LaneRadioFrequencyConfigured(configuration.Lane, null, configuration.FrequencyInMhz.Value));
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, laneEnabled.LaneNumber));

            if (timerLaneConfiguration == null || !timerLaneConfiguration.IsEnabled)
            {
                await _publisher.Dispatch(new TimerLaneEnabled(timer.Id, laneEnabled.LaneNumber));
            }
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, laneDisabled.LaneNumber));

            if (timerLaneConfiguration == null || timerLaneConfiguration.IsEnabled)
            {
                await _publisher.Dispatch(new TimerLaneDisabled(timer.Id, laneDisabled.LaneNumber));
            }
        }
    }
    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>();

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, frequencyChange.LaneNumber));

            if (timerLaneConfiguration == null || timerLaneConfiguration.FrequencyInMhz != frequencyChange.FrequencyInMhz)
            {
                await _publisher.Dispatch(new TimerLaneRadioFrequencyConfigured(timer.Id, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz));
            }
        }
    }
}
