namespace NaeTime.Messages.Events.Entities;
public record TrackDetailsChanged(Guid Id, string? Name, long MinimumLapMilliseconds, long MaximumLapMilliseconds, IEnumerable<Guid> Timers);

