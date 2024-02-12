namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeSessionConfigured(Guid SessionId, string Name, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);