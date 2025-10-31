using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;

public class TrackQueryHandler : ITrackQueryHandler
{
    private readonly ITrackProjection _trackProjection;

    public TrackQueryHandler(ITrackProjection trackProjection)
    {
        _trackProjection = trackProjection;
    }

    public Task<IEnumerable<Track>> GetAllTracks() => _trackProjection.GetTracks();

    public Task<Track?> GetTrack(Guid id) => _trackProjection.GetTrack(id);
}