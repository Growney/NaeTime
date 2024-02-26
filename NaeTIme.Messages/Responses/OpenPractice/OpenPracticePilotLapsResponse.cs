namespace NaeTime.Messages.Responses;
public record OpenPracticePilotLapsResponse(IEnumerable<OpenPracticePilotLapsResponse.Lap> Laps)
{
    public record Lap(Guid Id, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
}
