namespace NaeTime.Client.Razor.Lib.Models;
public record TrackDetails(Guid Id, string Name, long MinimumLapTimeMilliseconds, long? MaximumLapTimeMilliseconds, IEnumerable<Guid> Timers, byte AllowedLanes);
