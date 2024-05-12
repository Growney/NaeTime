using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class SingleLapsLeaderboardService
{
    private readonly OpenPracticeDbContext _dbContext;

    public SingleLapsLeaderboardService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<Messages.Models.SingleLapLeaderboardPosition>> GetOpenPracticeSessionSingleLapLeaderboardPositions(Guid sessionId)
    {
        var positions = await _dbContext.SingleLapLeaderboardPositions.Where(x => x.SessionId == sessionId).ToListAsync();

        return positions.Select(x => new Messages.Models.SingleLapLeaderboardPosition(x.Position, x.PilotId, x.TotalMilliseconds, x.CompletionUtc, x.LapId));
    }

    public async Task<Messages.Models.SingleLapRecord?> GetPilotOpenPracticeSessionSingleLapRecord(Guid sessionId, Guid pilotId)
    {
        var position = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        return position == null
            ? null
            : new Messages.Models.SingleLapRecord(position.TotalMilliseconds, position.CompletionUtc, position.LapId);
    }

    public async Task When(SingleLapLeaderboardPositionRemoved removed)
    {
        var existing = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId);

        if (existing == null)
        {
            return;
        }

        _dbContext.Remove(existing);

        await _dbContext.SaveChangesAsync();
    }
    public Task When(SingleLapLeaderboardRecordReduced reduced)
        => UpdateLapPosition(reduced.SessionId, null, reduced.PilotId, reduced.TotalMilliseconds, reduced.CompletionUtc, reduced.LapId);
    public Task When(SingleLapLeaderboardRecordImproved improved)
        => UpdateLapPosition(improved.SessionId, null, improved.PilotId, improved.TotalMilliseconds, improved.CompletionUtc, improved.LapId);
    public Task When(SingleLapLeaderboardPositionImproved improved)
        => UpdateLapPosition(improved.SessionId, improved.NewPosition, improved.PilotId, improved.TotalMilliseconds, improved.CompletionUtc, improved.LapId);
    public Task When(SingleLapLeaderboardPositionReduced reduced)
        => UpdateLapPosition(reduced.SessionId, reduced.NewPosition, reduced.PilotId, reduced.TotalMilliseconds, reduced.CompletionUtc, reduced.LapId);
    private async Task UpdateLapPosition(Guid sessionId, int? position, Guid pilotId, long totalMilliseconds, DateTime lastLapCompletionUtc, Guid lapId)
    {
        var existing = await _dbContext.SingleLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId);

        if (existing == null)
        {
            existing = new SingleLapLeaderboardPosition
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                PilotId = pilotId,
            };
            _dbContext.SingleLapLeaderboardPositions.Add(existing);
        }

        existing.Position = position ?? existing.Position;
        existing.TotalMilliseconds = totalMilliseconds;
        existing.CompletionUtc = lastLapCompletionUtc;
        existing.LapId = lapId;

        await _dbContext.SaveChangesAsync();
    }
}
