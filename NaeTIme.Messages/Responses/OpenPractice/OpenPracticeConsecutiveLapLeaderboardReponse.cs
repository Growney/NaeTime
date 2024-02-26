namespace NaeTime.Messages.Responses.OpenPractice;
public record OpenPracticeConsecutiveLapLeaderboardReponse(IEnumerable<OpenPracticeConsecutiveLapLeaderboardReponse.LeadboardPosition> Positions)
{
    public record LeadboardPosition(int Position, Guid PilotId, uint TotalLaps, long TotalMilliseconds, DateTime LastLapCompletionUtc, IEnumerable<Guid> IncludedLaps);
}