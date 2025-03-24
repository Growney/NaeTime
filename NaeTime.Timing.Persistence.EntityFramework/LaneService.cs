namespace NaeTime.Timing.Persistence.EntityFramework;
internal class LaneService
{
    private readonly NaeTimeDbContext _dbContext;
    public LaneService(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task When(LaneRadioFrequencyConfigured laneRadioFrequencyConfigured)
    {
        Lane? existing = await _dbContext.Lanes.FindAsync(laneRadioFrequencyConfigured.LaneNumber).ConfigureAwait(false);
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
        Lane? existing = await _dbContext.Lanes.FindAsync(laneEnabled.LaneNumber).ConfigureAwait(false);
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
        Lane? existing = await _dbContext.Lanes.FindAsync(laneDisabled.LaneNumber).ConfigureAwait(false);
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
