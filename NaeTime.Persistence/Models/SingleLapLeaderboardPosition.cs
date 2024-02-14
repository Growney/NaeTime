namespace NaeTime.Persistence.Models;

public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, Guid LapId, long LapMilliseconds, DateTime CompletionUtc);

