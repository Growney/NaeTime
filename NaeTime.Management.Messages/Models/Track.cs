namespace NaeTime.Management.Messages.Models;
public record Track(Guid Id, string Name, long MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds, List<Guid> Timers, byte AllowedLanes);