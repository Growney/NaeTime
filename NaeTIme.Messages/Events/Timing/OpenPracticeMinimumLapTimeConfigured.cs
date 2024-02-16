namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeMinimumLapTimeConfigured(Guid SessionId, long MinimumLapMilliseconds);