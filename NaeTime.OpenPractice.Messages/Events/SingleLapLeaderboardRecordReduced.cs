namespace NaeTime.OpenPractice.Messages.Events;
public record SingleLapLeaderboardRecordReduced(Guid SessionId, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);