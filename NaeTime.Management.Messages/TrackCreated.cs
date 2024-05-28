namespace NaeTime.Management.Messages;
public record TrackCreated(Guid Id, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> Timers, byte MaxLanes);

