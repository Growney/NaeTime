using NaeTime.Hardware.Messages.Messages;
using NaeTime.Hardware.Messages.Requests;
using NaeTime.Hardware.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;
using NaeTime.Timing.Messages.Requests;
using NaeTime.Timing.Messages.Responses;

namespace NaeTime.Hardware;
public class TimerLaneService : ISubscriber
{
    private readonly IPublishSubscribe _publisher;

    public TimerLaneService(IPublishSubscribe publisher)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
    }
    public async Task When(TimerConnectionEstablished connectionEstablished)
    {
        var laneConfigurations = await _publisher.Request<ActiveLanesConfigurationRequest, ActiveLanesConfigurationResponse>().ConfigureAwait(false);
        var timerLaneConfigurationReponse = await _publisher.Request<TimerLanesConfigurationRequest, TimerLanesConfigurationResponse>(new TimerLanesConfigurationRequest(connectionEstablished.TimerId)).ConfigureAwait(false);

        //We have no lane configurations update the configurations with those from the timer
        if (laneConfigurations == null || !laneConfigurations.Lanes.Any())
        {
            if (timerLaneConfigurationReponse != null && timerLaneConfigurationReponse.Lanes.Any())
            {
                foreach (var timerLane in timerLaneConfigurationReponse.Lanes)
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

    private async Task ReconfigurationLocalFromTimer(ActiveLanesConfigurationResponse laneConfigurations, TimerLanesConfigurationResponse timerLaneConfigurationReponse)
    {
        foreach (var timerLane in timerLaneConfigurationReponse.Lanes)
        {
            //The lane is not configured locally
            if (!laneConfigurations.Lanes.Any(x => x.Lane == timerLane.Lane))
            {
                await GenerateConfigurationEvents(timerLane).ConfigureAwait(false);
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
                 new TimersLaneConfigured(connectionEstablished.TimerId, timerReconfigurations)).ConfigureAwait(false);
        }
    }
    public async Task GenerateConfigurationEvents(TimerLanesConfigurationResponse.TimerLaneConfiguration configuration)
    {
        if (configuration.IsEnabled)
        {
            await _publisher.Dispatch(new LaneEnabled(configuration.Lane)).ConfigureAwait(false);
        }
        else
        {
            await _publisher.Dispatch(new LaneDisabled(configuration.Lane)).ConfigureAwait(false);
        }

        if (configuration.FrequencyInMhz != null)
        {
            await _publisher.Dispatch(new LaneRadioFrequencyConfigured(configuration.Lane, null, configuration.FrequencyInMhz.Value)).ConfigureAwait(false);
        }
    }

    public async Task When(LaneEnabled laneEnabled)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>().ConfigureAwait(false);

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, laneEnabled.LaneNumber));

            if (timerLaneConfiguration == null || !timerLaneConfiguration.IsEnabled)
            {
                await _publisher.Dispatch(new TimerLaneEnabled(timer.Id, laneEnabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>().ConfigureAwait(false);

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, laneDisabled.LaneNumber)).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.IsEnabled)
            {
                await _publisher.Dispatch(new TimerLaneDisabled(timer.Id, laneDisabled.LaneNumber)).ConfigureAwait(false);
            }
        }
    }
    public async Task When(LaneRadioFrequencyConfigured frequencyChange)
    {
        var timers = await _publisher.Request<TimerDetailsRequest, TimerDetailsResponse>().ConfigureAwait(false);

        if (timers == null)
        {
            return;
        }

        foreach (var timer in timers.Timers)
        {
            var timerLaneConfiguration = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(timer.Id, frequencyChange.LaneNumber)).ConfigureAwait(false);

            if (timerLaneConfiguration == null || timerLaneConfiguration.FrequencyInMhz != frequencyChange.FrequencyInMhz)
            {
                await _publisher.Dispatch(new TimerLaneRadioFrequencyConfigured(timer.Id, frequencyChange.LaneNumber, frequencyChange.FrequencyInMhz)).ConfigureAwait(false);
            }
        }
    }
}
