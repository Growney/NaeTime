namespace NaeTime.OpenPractice.Messages.Events;

public record ConsecutiveLapLeaderboardPositionReduced(Guid SessionId, uint LapCap, int NewPosition, int OldPosition, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps, bool WasTriggeredOnLapCompletion);