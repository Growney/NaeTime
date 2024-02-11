﻿namespace NaeTime.Messages.Events.Timing;
public record OpenPracticeSingleLapLeaderboardPositionsChanged(Guid SessionId, Guid LeaderboardId, IEnumerable<OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition> OldPositions, IEnumerable<OpenPracticeSingleLapLeaderboardPositionsChanged.SingleLapLeaderboardPosition> NewPositions)
{
    public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, uint LapNumber, long LapMilliseconds);
}