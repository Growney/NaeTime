using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface ITrackProjection
{
    Task<Track?> GetTrack(Guid id);
    Task<IEnumerable<Track>> GetTracks();
}