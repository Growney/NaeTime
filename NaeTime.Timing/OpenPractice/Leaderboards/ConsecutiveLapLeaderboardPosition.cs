namespace NaeTime.Timing.OpenPractice.Leaderboards;
public record ConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds);
