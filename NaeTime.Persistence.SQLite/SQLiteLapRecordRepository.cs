using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteLapRecordRepository : ILapRecordRepository
{
    private readonly NaeTimeDbContext _dbContext;
    public SQLiteLapRecordRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task AddOrUpdateConsecutiveLapRecord(Guid sessionId, Guid pilotId, uint lapCap, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps)
    {
        var existingRecord = await _dbContext.ConsecutiveLapRecords.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.PilotId == pilotId && x.LapCap == lapCap).ConfigureAwait(false);

        if (existingRecord == null)
        {
            existingRecord = new Models.ConsecutiveLapRecord()
            {
                Id = Guid.NewGuid(),
                SessionId = sessionId,
                PilotId = pilotId,
                LapCap = lapCap,
            };
            _dbContext.ConsecutiveLapRecords.Add(existingRecord);
        }

        existingRecord.TotalLaps = totalLaps;
        existingRecord.TotalMilliseconds = totalMilliseconds;
        existingRecord.LastLapCompletionUtc = lastLapCompletionUtc;
        existingRecord.IncludedLaps = includedLaps.Select(x => new Models.IncludedLap() { Id = Guid.NewGuid(), LapId = x }).ToList();

        await _dbContext.SaveChangesAsync();
    }
    public async Task<IEnumerable<ConsecutiveLapRecord>> GetPilotConsecutiveLapRecords(Guid sessionId, Guid pilotId)
    {
        var records = await _dbContext.ConsecutiveLapRecords.Where(x => x.SessionId == sessionId && x.PilotId == pilotId).ToListAsync().ConfigureAwait(false);

        return records.Select(x => new ConsecutiveLapRecord(x.SessionId, x.PilotId, x.LapCap, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(y => y.LapId)));
    }
    public Task RemoveConsecutiveLapRecord(Guid sessionId, Guid pilotId, uint lapCap) => _dbContext.ConsecutiveLapRecords.Where(x => x.SessionId == sessionId && x.PilotId == pilotId && x.LapCap == lapCap).ExecuteDeleteAsync();
}
