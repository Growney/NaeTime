namespace NaeTime.Messages.Events.Entities;
public record TrackCreated(Guid Id, string Name, long MinimumLapMilliseconds, long MaximumLapMilliseconds, IEnumerable<Guid> Timers);

