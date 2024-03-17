namespace NaeTime.OpenPractice.Messages.Events;
public record SingleLapLeaderboardRecordImproved(Guid SessionId, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);
