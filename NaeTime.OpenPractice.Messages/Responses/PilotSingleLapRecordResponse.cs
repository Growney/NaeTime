namespace NaeTime.OpenPractice.Messages.Responses;
public record PilotSingleLapRecordResponse(Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);