namespace NaeTime.OpenPractice.Messages.Models;
public record SingleLapRecord(long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);