namespace NaeTime.OpenPractice.Messages.Responses;
public record OpenPracticeSessionsResponse(IEnumerable<OpenPracticeSessionsResponse.OpenPracticeSession> Sessions)
{

    public record OpenPracticeSession(Guid Id, Guid TrackId, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<Lap> Laps, IEnumerable<PilotLane> ActiveLanes, IEnumerable<uint> TrackedConsecutiveLaps);
    public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
    public record PilotLane(Guid PilotId, byte Lane);
}