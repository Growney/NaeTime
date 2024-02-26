namespace NaeTime.Messages.Responses;
public record OpenPracticeLapsResponse(IEnumerable<OpenPracticeLapsResponse.Lap> Laps)
{
    public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
}
