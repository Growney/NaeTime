namespace NaeTime.Timing.OpenPractice.Leaderboards;
public record ConsecutiveLapsLeaderboardPosition(uint Position, Guid PilotId, uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletion);
