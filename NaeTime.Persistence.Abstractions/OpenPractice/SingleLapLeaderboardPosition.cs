namespace NaeTime.Persistence.Abstractions.OpenPractice;
public record SingleLapLeaderboardPosition(int Position, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);