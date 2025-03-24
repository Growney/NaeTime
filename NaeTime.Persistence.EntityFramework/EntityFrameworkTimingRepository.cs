using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.Persistence.EntityFramework.Models;

namespace NaeTime.Persistence.EntityFramework;

public class EntityFrameworkTimingRepository : ITimingRepository
{
    private readonly NaeTimeDbContext _dbContext;
    public EntityFrameworkTimingRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<ActiveLaneConfiguration>> GetActiveLaneConfigurations() =>
        await _dbContext.Lanes.Select(x => new ActiveLaneConfiguration(x.Id, x.BandId, x.FrequencyInMhz, x.IsEnabled)).ToListAsync();
    public async Task<IEnumerable<LaneActiveTimings>> GetSessionActiveTimings(Guid sessionId)
    {
        var timings = await _dbContext.ActiveTimings.Where(x => x.SessionId == sessionId).ToListAsync();

        List<LaneActiveTimings> responseData = new();

        foreach (ActiveTimings? timing in timings)
        {
            Abstractions.Timing.ActiveLap? lap = null;
            if (timing.ActiveLap != null)
            {
                lap = new Abstractions.Timing.ActiveLap((long)timing.ActiveLap.StartedSoftwareTime, (DateTime)timing.ActiveLap.StartedUtcTime, (ulong?)timing.ActiveLap.StartedHardwareTime);
            }

            Abstractions.Timing.ActiveSplit? split = null;
            if (timing.ActiveSplit != null)
            {
                split = new Abstractions.Timing.ActiveSplit((byte)timing.ActiveSplit.SplitNumber, (long)timing.ActiveSplit.StartedSoftwareTime, (DateTime)timing.ActiveSplit.StartedUtcTime);
            }

            responseData.Add(new LaneActiveTimings(timing.Lane, timing.LapNumber, lap, split));
        }

        return responseData;
    }
    public async Task<LaneActiveTimings?> GetSessionLaneActiveTimings(Guid sessionId, byte laneId)
    {
        var activeTimings = await _dbContext.ActiveTimings.FirstOrDefaultAsync(x => x.SessionId == sessionId && x.Lane == laneId);
        if (activeTimings == null)
        {
            return null;
        }

        Abstractions.Timing.ActiveLap? lap = null;
        if (activeTimings.ActiveLap != null)
        {
            lap = new Abstractions.Timing.ActiveLap(activeTimings.ActiveLap.StartedSoftwareTime, activeTimings.ActiveLap.StartedUtcTime, activeTimings.ActiveLap.StartedHardwareTime);
        }

        Abstractions.Timing.ActiveSplit? split = null;
        if (activeTimings.ActiveSplit != null)
        {
            split = new Abstractions.Timing.ActiveSplit(activeTimings.ActiveSplit.SplitNumber, activeTimings.ActiveSplit.StartedSoftwareTime, activeTimings.ActiveSplit.StartedUtcTime);
        }

        return new LaneActiveTimings(activeTimings.Lane, activeTimings.LapNumber, lap, split);
    }
}
