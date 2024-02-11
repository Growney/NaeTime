namespace NaeTime.Timing.OpenPractice.Leaderboards;
public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, uint LapNumber, long LapMilliseconds, DateTime CompletionTime);
