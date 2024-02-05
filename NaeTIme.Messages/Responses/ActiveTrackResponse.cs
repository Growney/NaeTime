namespace NaeTime.Messages.Responses;
public record ActiveTrackResponse(Guid TrackId, long MinimumLapMilliseconds, long MaximumLapMilliseconds, IEnumerable<Guid> Timers);
