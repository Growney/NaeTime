namespace NaeTime.OpenPractice.Messages.Events;

public record SingleLapLeaderboardPositionReduced(Guid SessionId, int NewPosition, int OldPosition, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);