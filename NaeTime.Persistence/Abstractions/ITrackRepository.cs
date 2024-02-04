namespace NaeTime.Persistence.Abstractions;
public interface ITrackRepository
{
    public Task AddOrUpdateTrack(Guid id, string name, long minimumLapMilliseconds, long maximumLapMilliseconds, IEnumerable<Guid> timers);
}
