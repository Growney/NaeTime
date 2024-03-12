namespace NaeTime.OpenPractice.Messages.Responses;
public record SingleLapLeaderboardResponse(IEnumerable<SingleLapLeaderboardResponse.LeadboardPosition> Positions)
{
    public record LeadboardPosition(int Position, Guid PilotId, long TotalMilliseconds, DateTime CompletionUtc, Guid LapId);
}