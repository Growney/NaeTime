using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class AverageLapLeaderboardService
{
    private readonly OpenPracticeDbContext _dbContext;

    public AverageLapLeaderboardService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Messages.Models.AverageLapLeaderboardPosition>> GetOpenPracticeSessionAverageLapLeaderboardPositions(Guid sessionId)
    {
        List<AverageLapLeaderboardPosition> positions = await _dbContext.AverageLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new Messages.Models.AverageLapLeaderboardPosition(x.Position, x.PilotId, x.AverageMilliseconds, x.FirstLapCompletionUtc));
    }

    public async Task<Messages.Models.AverageLapRecord?> GetPilotOpenPracticeSessionAverageLapRecord(Guid sessionId, Guid pilotId)
    {
        AverageLapLeaderboardPosition? position = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new Messages.Models.AverageLapRecord(position.AverageMilliseconds, position.FirstLapCompletionUtc);
    }

    public async Task When(AverageLapLeaderboardPositionRemoved removed)
    {
        Models.AverageLapLeaderboardPosition? existing = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId);

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
        Models.AverageLapLeaderboardPosition? existing = await _dbContext.AverageLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        if (existing == null)
        {
            existing = new Models.AverageLapLeaderboardPosition
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
