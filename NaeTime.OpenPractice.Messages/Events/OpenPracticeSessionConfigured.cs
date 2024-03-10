namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeSessionConfigured(Guid SessionId, string Name, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);