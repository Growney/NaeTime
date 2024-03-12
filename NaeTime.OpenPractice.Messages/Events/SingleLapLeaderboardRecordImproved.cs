namespace NaeTime.OpenPractice.Messages.Events;
public record SingleLapLeaderboardRecordImproved(Guid SessionId, int NewPosition, int OldPosition, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);
