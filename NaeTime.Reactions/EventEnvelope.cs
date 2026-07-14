namespace NaeTime.Reactions;

public sealed record EventEnvelope(Type EventType, object Event, DateTimeOffset OccurredAtUtc);
