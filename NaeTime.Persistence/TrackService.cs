using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
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
        var trackRepository = await _repositoryFactory.CreateTrackRepository().ConfigureAwait(false);

        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository().ConfigureAwait(false);

        var timerDetails = await hardwareRepository.GetTimerDetails(trackCreated.Timers).ConfigureAwait(false);

        var maxLanes = timerDetails.Min(x => x.AllowedLanes);

        await trackRepository.AddOrUpdateTrack(trackCreated.Id, trackCreated.Name, trackCreated.MinimumLapMilliseconds, trackCreated.MaximumLapMilliseconds, trackCreated.Timers, maxLanes).ConfigureAwait(false);
    }

    public async Task<TracksResponse> On(TracksRequest request)
    {
        var trackRepository = await _repositoryFactory.CreateTrackRepository().ConfigureAwait(false);

        var tracks = await trackRepository.GetAll().ConfigureAwait(false);

        return new TracksResponse(tracks.Select(x => new TracksResponse.Track(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));
    }

    public async Task<TrackResponse?> On(TrackRequest request)
    {
        var trackRepository = await _repositoryFactory.CreateTrackRepository().ConfigureAwait(false);

        var track = await trackRepository.Get(request.Id).ConfigureAwait(false);

        return track == null
            ? null
            : new TrackResponse(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, track.AllowedLanes);
    }
}
