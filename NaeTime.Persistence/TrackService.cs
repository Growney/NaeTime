using NaeTime.Messages.Events.Entities;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class TrackService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public TrackService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task When(TrackCreated trackCreated)
    {
        var trackRepository = await _repositoryFactory.CreateTrackRepository();
        await trackRepository.AddOrUpdateTrack(trackCreated.Id, trackCreated.Name, trackCreated.MinimumLapMilliseconds, trackCreated.MaximumLapMilliseconds, trackCreated.Timers);
    }
}
