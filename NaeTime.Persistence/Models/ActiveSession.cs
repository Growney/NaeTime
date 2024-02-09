namespace NaeTime.Persistence.Models;
public record ActiveSession(Guid SessionId, SessionType Type, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);