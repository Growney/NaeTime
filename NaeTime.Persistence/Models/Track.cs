namespace NaeTime.Persistence.Models;
public record Track(Guid Id, string Name, long MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds, IEnumerable<Guid> Timers, byte AllowedLanes);

