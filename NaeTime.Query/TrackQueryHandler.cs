using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class TrackQueryHandler : ITrackQueryHandler
{
    private readonly TrackList _trackList;

    public TrackQueryHandler(TrackList trackList)
    {
        _trackList = trackList;
    }

    public Task<IEnumerable<Track>> GetAllTracks()
    {
        throw new NotImplementedException();
    }

    public Task<Track?> GetTrack(Guid id) => _trackList.GetTrack(id);
}