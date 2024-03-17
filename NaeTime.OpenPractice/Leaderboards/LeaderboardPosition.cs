namespace NaeTime.OpenPractice.Leaderboards;
public record LeaderboardPosition<TRecord>(Guid PilotId, int Position, TRecord Record);