namespace NaeTime.Management.Messages.Responses;
public record TrackResponse(Guid Id, string Name, long MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds, IEnumerable<Guid> Timers, byte AllowedLanes);
