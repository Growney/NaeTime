using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.SQlite.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
internal class PilotApiClient : IPilotApiClient
{
    private readonly NaeTimeDbContext _context;

    public PilotApiClient(NaeTimeDbContext context)
    {
        _context = context;
    }
    public async Task<Pilot?> CreateAsync(string? firstname, string? lastname, string? callsign)
    {
        var domain = new PilotDetails
        {
            Id = Guid.NewGuid(),
            FirstName = firstname,
            LastName = lastname,
            CallSign = callsign
        };

        _context.Pilots.Add(domain);

        await _context.SaveChangesAsync();

        return new Pilot(domain.Id, domain.FirstName, domain.LastName, domain.CallSign);
    }

    public async Task<IEnumerable<Pilot>> GetAllAsync()
    {
        return await _context.Pilots.Select(x => new Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)).ToListAsync();
    }

    public Task<Pilot?> GetAsync(Guid id)
    {
        return _context.Pilots.Where(x => x.Id == id).Select(x => new Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)).FirstOrDefaultAsync();
    }

    public async Task<Pilot?> UpdateAsync(Pilot update)
    {
        var existing = _context.Pilots.FirstOrDefault(x => x.Id == update.Id);

        if (existing == null)
        {
            return null;
        }

        existing.FirstName = update.FirstName;
        existing.LastName = update.LastName;
        existing.CallSign = update.CallSign;

        _context.Pilots.Update(existing);

        await _context.SaveChangesAsync();

        return new Pilot(existing.Id, existing.FirstName, existing.LastName, existing.CallSign);
    }
}