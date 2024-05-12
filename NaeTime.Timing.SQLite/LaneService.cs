using NaeTime.Timing.Messages.Models;

namespace NaeTime.OpenPractice.SQLite;
internal class LaneService
{
    private readonly TimingDbContext _dbContext;
    public LaneService(TimingDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<IEnumerable<ActiveLaneConfiguration>> GetActiveLaneConfigurations() =>
        await _dbContext.Lanes.Select(x => new ActiveLaneConfiguration(x.Id, x.BandId, x.FrequencyInMhz, x.IsEnabled)).ToListAsync();
    public async Task When(LaneRadioFrequencyConfigured laneRadioFrequencyConfigured)
    {
        var existing = await _dbContext.Lanes.FindAsync(laneRadioFrequencyConfigured.LaneNumber).ConfigureAwait(false);
        if (existing == null)
        {
            existing = new Lane
            {
                Id = laneRadioFrequencyConfigured.LaneNumber,
                IsEnabled = true,
            };

            _dbContext.Lanes.Add(existing);
        }

        existing.BandId = laneRadioFrequencyConfigured.BandId;
        existing.FrequencyInMhz = laneRadioFrequencyConfigured.FrequencyInMhz;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(LaneEnabled laneEnabled)
    {
        var existing = await _dbContext.Lanes.FindAsync(laneEnabled.LaneNumber).ConfigureAwait(false);
        if (existing == null)
        {
            existing = new Lane
            {
                Id = laneEnabled.LaneNumber,
                FrequencyInMhz = 5800,
            };

            _dbContext.Lanes.Add(existing);
        }

        existing.IsEnabled = true;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
    public async Task When(LaneDisabled laneDisabled)
    {
        var existing = await _dbContext.Lanes.FindAsync(laneDisabled.LaneNumber).ConfigureAwait(false);
        if (existing == null)
        {
            existing = new Lane
            {
                Id = laneDisabled.LaneNumber,
                FrequencyInMhz = 5800,
            };

            _dbContext.Lanes.Add(existing);
        }

        existing.IsEnabled = false;

        await _dbContext.SaveChangesAsync().ConfigureAwait(false);
    }
}
