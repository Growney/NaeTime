namespace NaeTime.OpenPractice.Messages.Events;
public record ConsecutiveLapLeaderboardPositionRemoved(Guid SessionId, uint LapCap, Guid PilotId);