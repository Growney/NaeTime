namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record TotalLapLeaderboardPosition(int Position, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);