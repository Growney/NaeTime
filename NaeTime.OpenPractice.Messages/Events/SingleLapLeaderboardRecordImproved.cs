namespace NaeTime.OpenPractice.Messages.Events;
public record SingleLapLeaderboardRecordImproved(Guid SessionId, int Position, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);
