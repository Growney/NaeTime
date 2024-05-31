namespace NaeTime.OpenPractice.Messages.Models;
public record AverageLapLeaderboardPosition(int Position, Guid PilotId, double AverageMilliseconds, DateTime FirstLapCompletion);