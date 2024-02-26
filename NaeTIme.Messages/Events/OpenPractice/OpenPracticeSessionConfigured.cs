namespace NaeTime.Messages.Events.OpenPractice;
public record OpenPracticeSessionConfigured(Guid SessionId, string Name, Guid TrackId, long MinimumLapMilliseconds, long? MaximumLapMilliseconds);