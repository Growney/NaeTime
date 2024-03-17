namespace NaeTime.OpenPractice.Messages.Events;
public record TotalLapLeaderboardRecordImproved(Guid SessionId, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);
