namespace NaeTime.Messages.Responses;
public record OpenPracticeSessionResponse(Guid SessionId, Guid TrackId, string? Name, long MinimumLapMilliseconds, long? MaximumLapMilliseconds, IEnumerable<OpenPracticeSessionResponse.Lap> Laps,
    IEnumerable<OpenPracticeSessionResponse.PilotLane> ActiveLanes, IEnumerable<uint> TrackedConsecutiveLaps)
{
    public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
    public record PilotLane(Guid PilotId, byte Lane);
}