namespace NaeTime.OpenPractice.Messages.Responses;
public record TotalLapsLeaderboardResponse(IEnumerable<TotalLapsLeaderboardResponse.LeadboardPosition> Positions)
{
    public record LeadboardPosition(int Position, Guid PilotId, int TotalLaps, DateTime FirstLapCompletionUtc);
}
