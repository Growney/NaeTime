namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeConsecutiveLapLeaderboardConfigured(Guid SessionId, Guid LeaderboardId, uint ConsecutiveLaps);