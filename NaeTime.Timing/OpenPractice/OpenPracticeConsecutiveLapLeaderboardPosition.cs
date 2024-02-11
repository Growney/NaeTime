namespace NaeTime.Timing.OpenPractice;
public record OpenPracticeConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds);
