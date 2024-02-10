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
    //TODO CHANGE THIS TO INITIALISE MISSING LANE CONFIGURATIONS INSTEAD OF ALL OF THEM
    public async Task When(TimerConnectionEstablished connectionEstablished)
    {
        var laneConfigurations = await _publisher.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();
        var timerLaneConfigurationReponse = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(connectionEstablished.TimerId));

        //We have no lane configurations update the configurations with those from the timer
        if (laneConfigurations == null || !laneConfigurations.Lanes.Any())
        {
            if (timerLaneConfigurationReponse != null && timerLaneConfigurationReponse.Lanes.Any())
            {
                foreach (var configuration in timerLaneConfigurationReponse.Lanes)
                {
                    await GenerateConfigurationEvents(configuration);
                }
            }
        }

        if (laneConfigurations == null || !laneConfigurations.Lanes.Any())
        {
            var timerLaneConfigurations = await _publisher.Request<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(new TimerLaneConfigurationRequest(connectionEstablished.TimerId));

            if (timerLaneConfigurations != null)
            {
                foreach (var configuration in timerLaneConfigurations.Lanes)
                {
                    await GenerateConfigurationEvents(configuration);
                }
            }
        }
        else
        {
            await _publisher.Dispatch(
                new TimersLaneConfigured(connectionEstablished.TimerId,
                laneConfigurations.Lanes.Select(x => new TimersLaneConfigured.LaneConfiguration(x.Lane, x.FrequencyInMhz, x.IsEnabled))));
        }
    }

    public async Task GenerateConfigurationEvents(TimerLaneConfigurationResponse.TimerLaneConfiguration configuration)
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
}
