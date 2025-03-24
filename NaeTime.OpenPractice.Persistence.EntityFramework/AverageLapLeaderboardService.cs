namespace NaeTime.OpenPractice.Persistence.EntityFramework;
internal class AverageLapLeaderboardService
{
    private readonly NaeTimeDbContext _dbContext;

    public AverageLapLeaderboardService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task When(AverageLapLeaderboardPositionRemoved removed)
    {
        AverageLapLeaderboardPosition? existing = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId);

        if (existing == null)
        {
            return;
        }

        _dbContext.Remove(existing);

        await _dbContext.SaveChangesAsync();
    }
    public Task When(AverageLapLeaderboardRecordReduced reduced)
        => UpdateLapPosition(reduced.SessionId, null, reduced.PilotId, reduced.AverageMilliseconds, reduced.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardRecordImproved improved)
        => UpdateLapPosition(improved.SessionId, null, improved.PilotId, improved.AverageMilliseconds, improved.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardPositionImproved improved)
        => UpdateLapPosition(improved.SessionId, improved.NewPosition, improved.PilotId, improved.AverageMilliseconds, improved.FirstLapCompletionUtc);
    public Task When(AverageLapLeaderboardPositionReduced reduced)
        => UpdateLapPosition(reduced.SessionId, reduced.NewPosition, reduced.PilotId, reduced.AverageMilliseconds, reduced.FirstLapCompletionUtc);
    private async Task UpdateLapPosition(Guid sessionId, int? position, Guid pilotId, double averageMilliseconds, DateTime firstLapCompletion)
    {
        AverageLapLeaderboardPosition? existing = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        if (existing == null)
        {
            existing = new AverageLapLeaderboardPosition
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                PilotId = pilotId,
            };
            _dbContext.AverageLapLeaderboardPositions.Add(existing);
        }

        existing.Position = position ?? existing.Position;
        existing.AverageMilliseconds = averageMilliseconds;
        existing.FirstLapCompletionUtc = firstLapCompletion;

        await _dbContext.SaveChangesAsync();
    }
}
