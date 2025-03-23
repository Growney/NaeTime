namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record AverageLapLeaderboardPosition(int Position, Guid PilotId, double AverageMilliseconds, DateTime FirstLapCompletion);