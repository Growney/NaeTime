using NaeTime.Messages.Events.Timing;
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
    public async Task<ActiveLaneConfigurationResponse?> On(ActiveLaneConfigurationRequest request)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository();

        var lanes = await laneRepository.GetLanes();

        return new ActiveLaneConfigurationResponse(lanes.Select(x => new ActiveLaneConfigurationResponse.LaneConfiguration(x.LaneNumber, x.BandId, x.FrequencyInMhz, x.IsEnabled)));
    }
    public async Task When(LaneRadioFrequencyConfigured laneRadioFrequencyConfigured)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository();

        await laneRepository.SetLaneRadioFrequency(laneRadioFrequencyConfigured.LaneNumber, laneRadioFrequencyConfigured.BandId, laneRadioFrequencyConfigured.FrequencyInMhz);
    }
    public async Task When(LaneEnabled laneEnabled)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository();
        await laneRepository.SetLaneStatus(laneEnabled.LaneNumber, true);
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var laneRepository = await _repositoryFactory.CreateLaneRepository();
        await laneRepository.SetLaneStatus(laneDisabled.LaneNumber, false);
    }
}
