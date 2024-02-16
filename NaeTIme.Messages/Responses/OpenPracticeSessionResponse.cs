namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionResponse(Guid SessionId, Guid TrackId, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<OpenPracticeSessionResponse.Lap> Laps,
    IEnumerable<OpenPracticeSessionResponse.PilotLane> ActiveLanes,
    IEnumerable<OpenPracticeSessionResponse.SingleLapLeaderboard> SingleLapLeaderboards,
    IEnumerable<OpenPracticeSessionResponse.ConsecutiveLapLeaderboard> ConsecutiveLapLeaderboards)
{
    public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
    public record PilotLane(Guid PilotId, byte Lane);
    public record ConsecutiveLapLeaderboard(Guid LeaderboardId, uint ConsecutiveLaps, IEnumerable<ConsecutiveLapLeaderboardPosition> Positions);
    public record ConsecutiveLapLeaderboardPosition(uint Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
    public record SingleLapLeaderboard(Guid LeaderboardId, IEnumerable<SingleLapLeaderboardPosition> Positions);
    public record SingleLapLeaderboardPosition(uint Position, Guid PilotId, Guid LapId, long TotalMilliseconds, DateTime CompletionUtc);
}