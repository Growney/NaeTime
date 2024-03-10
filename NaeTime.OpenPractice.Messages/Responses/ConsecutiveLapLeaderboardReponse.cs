namespace NaeTime.OpenPractice.Messages.Responses;
public record ConsecutiveLapLeaderboardReponse(IEnumerable<ConsecutiveLapLeaderboardReponse.LeadboardPosition> Positions)
{
    public record LeadboardPosition(int Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}