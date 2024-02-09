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
        var trackRepository = await _repositoryFactory.CreateTrackRepository();

        var hardwareRepository = await _repositoryFactory.CreateHardwareRepository();

        var timerDetails = await hardwareRepository.GetTimerDetails(trackCreated.Timers);

        var maxLanes = timerDetails.Min(x => x.AllowedLanes);

        await trackRepository.AddOrUpdateTrack(trackCreated.Id, trackCreated.Name, trackCreated.MinimumLapMilliseconds, trackCreated.MaximumLapMilliseconds, trackCreated.Timers, maxLanes);
    }

    public async Task<TracksResponse> On(TracksRequest request)
    {
        var trackRepository = await _repositoryFactory.CreateTrackRepository();

        var tracks = await trackRepository.GetAll();

        return new TracksResponse(tracks.Select(x => new TracksResponse.Track(x.Id, x.Name, x.MinimumLapTimeMilliseconds, x.MaximumLapTimeMilliseconds, x.Timers, x.AllowedLanes)));
    }

    public async Task<TrackResponse?> On(TrackRequest request)
    {
        var trackRepository = await _repositoryFactory.CreateTrackRepository();

        var track = await trackRepository.Get(request.Id);

        if (track == null)
        {
            return null;
        }

        return new TrackResponse(track.Id, track.Name, track.MinimumLapTimeMilliseconds, track.MaximumLapTimeMilliseconds, track.Timers, track.AllowedLanes);
    }
}
