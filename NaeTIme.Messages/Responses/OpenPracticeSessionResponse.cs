﻿namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionResponse(Guid SessionId, Guid TrackId, string? Name, IEnumerable<OpenPracticeSessionResponse.Lap> Laps,
    IEnumerable<OpenPracticeSessionResponse.PilotLane> ActiveLanes,
    IEnumerable<OpenPracticeSessionResponse.SingleLapLeaderboard> SingleLapLeaderboards,
    IEnumerable<OpenPracticeSessionResponse.ConsecutiveLapLeaderboard> ConsecutiveLapLeaderboards)
{
    public record Lap(Guid Id, Guid PilotId, uint LapNumber, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
    public record PilotLane(Guid PilotId, byte Lane);
    public record ConsecutiveLapLeaderboard(Guid LeaderboardId, uint ConsecutiveLaps, IEnumerable<ConsecutiveLapLeaderboardPosition> Positions);
    public record ConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
    public record SingleLapLeaderboard(Guid LeaderboardId, IEnumerable<SingleLapLeaderboardPosition> Positions);
    public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, uint LapNumber, long TotalMilliseconds, DateTime CompletionUtc);
}