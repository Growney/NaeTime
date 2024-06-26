﻿using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class ConsecutiveLapsLeaderboardService
{
    private readonly OpenPracticeDbContext _dbContext;

    public ConsecutiveLapsLeaderboardService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<Messages.Models.ConsecutiveLapRecord>> GetPilotOpenPracticeSessionConsecutiveLapRecords(Guid sessionId, Guid pilotId)
    {
        List<ConsecutiveLapLeaderboardPosition> positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync();

        return positions.Select(x => new Messages.Models.ConsecutiveLapRecord(x.LapCap, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }
    public async Task<IEnumerable<Messages.Models.ConsecutiveLapLeaderboardPosition>> GetOpenPracticeSessionConsecutiveLapsLeaderboardPositions(Guid sessionId, uint lapCap)
    {
        List<ConsecutiveLapLeaderboardPosition> positions = await _dbContext.ConsecutiveLapLeaderboardPositions.Where(x => x.SessionId == sessionId && x.LapCap == lapCap).ToListAsync();

        return positions.Select(x => new Messages.Models.ConsecutiveLapLeaderboardPosition(x.Position, x.PilotId, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(x => x.LapId)));
    }
    public Task When(ConsecutiveLapLeaderboardRecordReduced reduced)
        => UpdateLapPosition(reduced.SessionId, reduced.LapCap, null, reduced.PilotId, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps);
    public Task When(ConsecutiveLapLeaderboardRecordImproved improved)
        => UpdateLapPosition(improved.SessionId, improved.LapCap, null, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps);

    public Task When(ConsecutiveLapLeaderboardPositionImproved improved)
        => UpdateLapPosition(improved.SessionId, improved.LapCap, improved.NewPosition, improved.PilotId, improved.TotalLaps, improved.TotalMilliseconds, improved.LastLapCompletionUtc, improved.IncludedLaps);
    public Task When(ConsecutiveLapLeaderboardPositionReduced reduced)
        => UpdateLapPosition(reduced.SessionId, reduced.LapCap, reduced.NewPosition, reduced.PilotId, reduced.TotalLaps, reduced.TotalMilliseconds, reduced.LastLapCompletionUtc, reduced.IncludedLaps);
    public async Task When(ConsecutiveLapLeaderboardPositionRemoved removed)
    {
        ConsecutiveLapLeaderboardPosition? existing = await _dbContext.ConsecutiveLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId && x.LapCap == removed.LapCap);

        if (existing == null)
        {
            return;
        }

        _dbContext.Remove(existing);

        await _dbContext.SaveChangesAsync();
    }

    private async Task UpdateLapPosition(Guid sessionId, uint lapCap, int? position, Guid pilotId, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps)
    {
        ConsecutiveLapLeaderboardPosition? existing = await _dbContext.ConsecutiveLapLeaderboardPositions.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId && x.LapCap == lapCap);

        if (existing == null)
        {
            existing = new ConsecutiveLapLeaderboardPosition
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                LapCap = lapCap,
                PilotId = pilotId,
            };
            _dbContext.ConsecutiveLapLeaderboardPositions.Add(existing);
        }

        existing.Position = position ?? existing.Position;
        existing.TotalLaps = totalLaps;
        existing.TotalMilliseconds = totalMilliseconds;
        existing.LastLapCompletionUtc = lastLapCompletionUtc;
        existing.IncludedLaps = includedLaps.Select(x => new IncludedLap { Id = Guid.NewGuid(), LapId = x }).ToList();

        await _dbContext.SaveChangesAsync();
    }
}