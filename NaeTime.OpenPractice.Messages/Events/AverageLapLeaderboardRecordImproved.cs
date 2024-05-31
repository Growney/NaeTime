namespace NaeTime.OpenPractice.Messages.Events;
public record AverageLapLeaderboardRecordImproved(Guid SessionId, Guid PilotId, double AverageMilliseconds, DateTime FirstLapCompletionUtc);