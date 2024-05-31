namespace NaeTime.OpenPractice.Messages.Events;
public record AverageLapLeaderboardRecordReduced(Guid SessionId, Guid PilotId, double AverageMilliseconds, DateTime FirstLapCompletionUtc);
