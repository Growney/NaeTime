namespace NaeTime.Management.Messages.Messages;
public record TrackDetailsChanged(Guid Id, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Guid> Timers);

