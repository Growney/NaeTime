namespace NaeTime.OpenPractice.Messages.Models;
public record SingleLapLeaderboardPosition(int Position, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);