using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface ITrackRepository
{
    public Task<IEnumerable<Track>> GetAll();
    public Task<Track?> Get(Guid id);
    public Task AddOrUpdateTrack(Guid id, string name, long minimumLapMilliseconds, long? maximumLapMilliseconds, IEnumerable<Guid> timers, byte allowedLanes);
}
