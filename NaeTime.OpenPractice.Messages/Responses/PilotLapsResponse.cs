namespace NaeTime.OpenPractice.Messages.Responses;
public record PilotLapsResponse(IEnumerable<PilotLapsResponse.Lap> Laps)
{
    public record Lap(Guid Id, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
}
