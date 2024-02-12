namespace NaeTime.Persistence.Models;
public record ConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc);

