namespace NaeTime.OpenPractice.Messages.Events;
public record TotalLapLeaderboardPositionImproved(Guid SessionId, int NewPosition, int? OldPosition, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);
