namespace NaeTime.OpenPractice.Messages.Responses;
public record PilotSingleLapRecordResponse(long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);