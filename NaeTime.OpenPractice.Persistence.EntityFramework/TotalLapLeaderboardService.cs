namespace NaeTime.OpenPractice.Persistence.EntityFramework;
internal class TotalLapLeaderboardService
{
    private readonly NaeTimeDbContext _dbContext;

    public TotalLapLeaderboardService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task When(TotalLapsLeaderboardPositionRemoved removed)
    {
        TotalLapsLeaderboardPosition? existing = await _dbContext.TotalLapsLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId);

        if (existing == null)
        {
            return;
        }

        _dbContext.Remove(existing);

        await _dbContext.SaveChangesAsync();
    }

    public Task When(TotalLapsLeaderboardRecordReduced reduced)
        => UpdateLapPosition(reduced.SessionId, null, reduced.PilotId, reduced.TotalLaps, reduced.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardRecordImproved improved)
        => UpdateLapPosition(improved.SessionId, null, improved.PilotId, improved.TotalLaps, improved.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardPositionImproved improved)
        => UpdateLapPosition(improved.SessionId, improved.NewPosition, improved.PilotId, improved.TotalLaps, improved.FirstLapCompletionUtc);
    public Task When(TotalLapsLeaderboardPositionReduced reduced)
        => UpdateLapPosition(reduced.SessionId, reduced.NewPosition, reduced.PilotId, reduced.TotalLaps, reduced.FirstLapCompletionUtc);
    private async Task UpdateLapPosition(Guid sessionId, int? position, Guid pilotId, int totalLaps, DateTime firstLapCompletion)
    {
        TotalLapsLeaderboardPosition? existing = await _dbContext.TotalLapsLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        if (existing == null)
        {
            existing = new TotalLapsLeaderboardPosition
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                PilotId = pilotId,
            };
            _dbContext.TotalLapsLeaderboardPositions.Add(existing);
        }

        existing.Position = position ?? existing.Position;
        existing.TotalLaps = totalLaps;
        existing.FirstLapCompletionUtc = firstLapCompletion;

        await _dbContext.SaveChangesAsync();
    }
}
