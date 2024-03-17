namespace NaeTime.OpenPractice.Messages.Events;
public record TotalLapsLeaderboardRecordImproved(Guid SessionId, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);
