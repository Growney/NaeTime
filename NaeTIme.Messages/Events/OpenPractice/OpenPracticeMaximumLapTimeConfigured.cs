namespace NaeTime.Messages.Events.OpenPractice;

public record OpenPracticeMaximumLapTimeConfigured(Guid SessionId, long? MaximumLapMilliseconds);