namespace NaeTime.Messages.Events.Timing;

public record OpenPracticeMaximumLapTimeConfigured(Guid SessionId, long? MaximumLapMilliseconds);