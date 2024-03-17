namespace NaeTime.OpenPractice.Messages.Events;
public record TotalLapLeaderboardRecordReduced(Guid SessionId, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);