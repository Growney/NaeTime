namespace NaeTime.Timing.OpenPractice.Leaderboards;
public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, Guid LapId, long LapMilliseconds, DateTime CompletionTime);
