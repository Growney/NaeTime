using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface ITrackProjection
{
    Task<Track?> GetTrack(Guid id);
    Task<IEnumerable<Track>> GetTracks();
}