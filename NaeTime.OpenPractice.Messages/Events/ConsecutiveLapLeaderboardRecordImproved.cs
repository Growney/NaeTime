namespace NaeTime.OpenPractice.Messages.Events;
public record ConsecutiveLapLeaderboardRecordImproved(Guid SessionId, int Position, uint LapCap, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps, bool WasTriggeredOnLapCompletion);
