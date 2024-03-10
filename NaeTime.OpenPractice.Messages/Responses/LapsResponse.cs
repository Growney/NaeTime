namespace NaeTime.OpenPractice.Messages.Responses;
public record LapsResponse(IEnumerable<LapsResponse.Lap> Laps)
{
    public record Lap(Guid Id, Guid PilotId, DateTime StartedUtc, DateTime FinishedUtc, LapStatus Status, long TotalMilliseconds);
    public enum LapStatus
    {
        Invalid,
        Completed
    }
}
