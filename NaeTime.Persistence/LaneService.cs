﻿using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class LaneService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;


    public LaneService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }
    public async Task<ActiveLanesConfigurationResponse?> On(ActiveLanesConfigurationRequest request)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository().ConfigureAwait(false);

        var lanes = await laneRepository.GetLanes().ConfigureAwait(false);

        return new ActiveLanesConfigurationResponse(lanes.Select(x => new ActiveLanesConfigurationResponse.LaneConfiguration(x.LaneNumber, x.BandId, x.FrequencyInMhz, x.IsEnabled)));
    }
    public async Task When(LaneRadioFrequencyConfigured laneRadioFrequencyConfigured)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository().ConfigureAwait(false);

        await laneRepository.SetLaneRadioFrequency(laneRadioFrequencyConfigured.LaneNumber, laneRadioFrequencyConfigured.BandId, laneRadioFrequencyConfigured.FrequencyInMhz).ConfigureAwait(false);
    }
    public async Task When(LaneEnabled laneEnabled)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository().ConfigureAwait(false);
        await laneRepository.SetLaneStatus(laneEnabled.LaneNumber, true);
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository().ConfigureAwait(false);
        await laneRepository.SetLaneStatus(laneDisabled.LaneNumber, false);
    }
}
