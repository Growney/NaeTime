using NaeTime.OpenPractice.Messages.Events;

namespace NaeTime.OpenPractice.SQLite;
internal class ConsecutiveLapsService : ISubscriber
{
    private readonly OpenPracticeDbContext _dbContext;

    public ConsecutiveLapsService(OpenPracticeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<PilotConsecutiveLapRecordsResponse> On(PilotConsecutiveLapRecordsRequest request)
    {
        var records = await _dbContext.ConsecutiveLapRecords.Where(x => x.SessionId == request.SessionId && x.PilotId == request.PilotId).ToListAsync().ConfigureAwait(false);

        return new PilotConsecutiveLapRecordsResponse(
            records.Select(x => new PilotConsecutiveLapRecordsResponse.ConsecutiveLapRecord(x.LapCap, x.TotalLaps, x.TotalMilliseconds, x.LastLapCompletionUtc, x.IncludedLaps.Select(y => y.LapId).ToList())));
    }
    public async Task When(ConsecutiveLapRecordRemoved removed)
    {
        var record = await _dbContext.ConsecutiveLapRecords.FirstOrDefaultAsync(x => x.SessionId == removed.SessionId && x.PilotId == removed.PilotId && x.LapCap == removed.LapCap).ConfigureAwait(false);
        if (record == null)
        {
            return;
        }

        _dbContext.ConsecutiveLapRecords.Remove(record);

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(ConsecutiveLapRecordImproved improved)
    {
        var record = await _dbContext.ConsecutiveLapRecords.FirstOrDefaultAsync(x => x.SessionId == improved.SessionId && x.PilotId == improved.PilotId && x.LapCap == improved.LapCap).ConfigureAwait(false);

        if (record == null)
        {
            record = new ConsecutiveLapRecord
            {
                SessionId = improved.SessionId,
                PilotId = improved.PilotId,
                LapCap = improved.LapCap
            };
            _dbContext.ConsecutiveLapRecords.Add(record);
        }
        record.TotalLaps = improved.TotalLaps;
        record.TotalMilliseconds = improved.TotalMilliseconds;
        record.LastLapCompletionUtc = improved.LastLapCompletionUtc;
        record.IncludedLaps = improved.IncludedLaps.Select(x => new IncludedLap() { Id = Guid.NewGuid(), LapId = x }).ToList();

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(ConsecutiveLapRecordReduced reduced)
    {
        var record = await _dbContext.ConsecutiveLapRecords.FirstOrDefaultAsync(x => x.SessionId == reduced.SessionId && x.PilotId == reduced.PilotId && x.LapCap == reduced.LapCap).ConfigureAwait(false);

        if (record == null)
        {
            record = new ConsecutiveLapRecord
            {
                SessionId = reduced.SessionId,
                PilotId = reduced.PilotId,
                LapCap = reduced.LapCap
            };
            _dbContext.ConsecutiveLapRecords.Add(record);
        }
        record.TotalLaps = reduced.TotalLaps;
        record.TotalMilliseconds = reduced.TotalMilliseconds;
        record.LastLapCompletionUtc = reduced.LastLapCompletionUtc;
        record.IncludedLaps = reduced.IncludedLaps.Select(x => new IncludedLap() { Id = Guid.NewGuid(), LapId = x }).ToList();

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
