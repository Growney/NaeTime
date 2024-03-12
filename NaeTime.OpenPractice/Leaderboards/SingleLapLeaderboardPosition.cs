namespace NaeTime.OpenPractice.Leaderboards;
public record SingleLapLeaderboardPosition(int Position, Guid PilotId, Guid LapId, long LapMilliseconds, DateTime CompletionTime);
