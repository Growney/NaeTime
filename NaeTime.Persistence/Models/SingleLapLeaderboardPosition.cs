namespace NaeTime.Persistence.Models;

public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, uint LapNumber, long LapMilliseconds, DateTime CompletionUtc);

