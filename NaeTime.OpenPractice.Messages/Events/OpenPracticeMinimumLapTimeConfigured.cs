namespace NaeTime.OpenPractice.Messages.Events;
public record OpenPracticeMinimumLapTimeConfigured(Guid SessionId, long MinimumLapMilliseconds);