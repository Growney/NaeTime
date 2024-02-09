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
        var laneConfigurations = await _publisher.Request<ActiveLaneConfigurationRequest, ActiveLaneConfigurationResponse>();

        if (laneConfigurations == null || !laneConfigurations.Configurations.Any())
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
                laneConfigurations.Configurations.Select(x => new TimersLaneConfigured.LaneConfiguration(x.Lane, x.PilotId, x.FrequencyInMhz, x.IsEnabled))));
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

        //Only configure the lane do not clear pilots
        if (configuration.PilotId != null)
        {
            await _publisher.Dispatch(new LanePilotConfigured(configuration.Lane, configuration.PilotId.Value));
        }

        if (configuration.FrequencyInMhz != null)
        {
            await _publisher.Dispatch(new LaneRadioFrequencyConfigured(configuration.Lane, configuration.FrequencyInMhz.Value));
        }
    }
}
