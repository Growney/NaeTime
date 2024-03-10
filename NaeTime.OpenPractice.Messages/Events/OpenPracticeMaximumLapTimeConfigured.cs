namespace NaeTime.OpenPractice.Messages.Events;

public record OpenPracticeMaximumLapTimeConfigured(Guid SessionId, long? MaximumLapMilliseconds);