namespace NaeTime.Persistence.Models;
public record SessionDetails(Guid Id, string? Name, SessionType Type, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);