namespace NaeTime.OpenPractice.Messages.Events;
public record TotalLapsLeaderboardRecordReduced(Guid SessionId, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);