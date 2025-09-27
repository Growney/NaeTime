using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class TrackQueryHandler : ITrackQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;

    public TrackQueryHandler(IProjectionProvider projectionProvider)
    {
        _projectionProvider = projectionProvider;
    }

    public async Task<IEnumerable<Track>> GetAllTracks()
    {
        TrackList trackList = await _projectionProvider.Load<TrackList>();
        return await trackList.GetTracks();
    }

    public async Task<Track?> GetTrack(Guid id)
    {
        TrackList trackList = await _projectionProvider.Load<TrackList>();
        return await trackList.GetTrack(id);
    }
}