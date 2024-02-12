﻿namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeConsecutiveLapLeaderboardPositionsChanged(Guid SessionId, Guid LeaderboardId, IEnumerable<OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition> OldPositions, IEnumerable<OpenPracticeConsecutiveLapLeaderboardPositionsChanged.OpenPracticeConsecutiveLapLeaderboardPosition> NewPositions)
{
    public record OpenPracticeConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint StartLapNumber, uint EndLapNumber, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletion);
}
