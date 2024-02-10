using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLiteLaneRepository : ILaneRepository
{
    private readonly NaeTimeDbContext _dbContext;
    public SQLiteLaneRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task<IEnumerable<Lane>> GetLanes()
    {
        return await _dbContext.Lanes.Select(x => new Lane(x.Id, x.BandId, x.RadioFrequencyInMhz, x.IsEnabled)).ToListAsync();
    }


    public async Task SetLaneRadioFrequency(byte lane, byte? bandId, int frequencyInMhz)
    {
        var existingStatus = await _dbContext.Lanes.FirstOrDefaultAsync(x => x.Id == lane);

        if (existingStatus == null)
        {
            existingStatus = new Models.Lane
            {
                Id = lane,
                BandId = bandId,
                RadioFrequencyInMhz = frequencyInMhz,
                IsEnabled = true,
            };

            _dbContext.Lanes.Add(existingStatus);
        }
        else
        {
            existingStatus.RadioFrequencyInMhz = frequencyInMhz;
        }

        await _dbContext.SaveChangesAsync();
    }
    public Task SetLaneStatus(byte lane, bool isEnabled)
    {
        var existingStatus = _dbContext.Lanes.FirstOrDefault(x => x.Id == lane);

        if (existingStatus == null)
        {
            existingStatus = new Models.Lane
            {
                Id = lane,
                IsEnabled = isEnabled
            };

            _dbContext.Lanes.Add(existingStatus);
        }
        else
        {
            existingStatus.IsEnabled = isEnabled;
        }

        return _dbContext.SaveChangesAsync();
    }
}
