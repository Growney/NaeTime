using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface ITrackQueryHandler
{
    Task<Track?> GetTrack(Guid id);
}
