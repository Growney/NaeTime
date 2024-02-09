using Microsoft.EntityFrameworkCore;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Models;
using NaeTime.Persistence.SQLite.Context;

namespace NaeTime.Persistence.SQLite;
public class SQLitePilotRepository : IPilotRepository
{
    private readonly NaeTimeDbContext _dbContext;

    public SQLitePilotRepository(NaeTimeDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }
    public async Task AddOrUpdatePilot(Guid id, string firstName, string lastName, string callSign)
    {
        var existing = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            existing = new Models.Pilot
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                CallSign = callSign
            };

            _dbContext.Pilots.Add(existing);
        }
        else
        {
            existing.FirstName = firstName;
            existing.LastName = lastName;
            existing.CallSign = callSign;
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<Pilot?> GetPilot(Guid id)
    {
        var existing = await _dbContext.Pilots.FirstOrDefaultAsync(x => x.Id == id);

        if (existing == null)
        {
            return null;
        }

        return new Pilot(existing.Id, existing.FirstName, existing.LastName, existing.CallSign);
    }

    public async Task<IEnumerable<Pilot>> GetPilots()
    {
        return await _dbContext.Pilots.Select(x => new Persistence.Models.Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)).ToListAsync();
    }
}
