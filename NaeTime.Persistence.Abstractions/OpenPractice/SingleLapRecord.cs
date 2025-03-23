namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record SingleLapRecord(long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);