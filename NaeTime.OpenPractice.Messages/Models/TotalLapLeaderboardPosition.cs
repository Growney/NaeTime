namespace NaeTime.OpenPractice.Messages.Models;
public record TotalLapLeaderboardPosition(int Position, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);