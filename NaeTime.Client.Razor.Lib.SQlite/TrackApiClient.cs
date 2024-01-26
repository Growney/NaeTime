using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.SQlite.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
internal class TrackApiClient : ITrackApiClient
{
    private readonly NaeTimeDbContext _context;

    public TrackApiClient(NaeTimeDbContext context)
    {
        _context = context;
    }
    public async Task<Track?> CreateAsync(string name, IEnumerable<TimedGate> gates)
    {
        var gateDetails = new List<TimedGateDetails>();
        int position = -1;
        foreach (var gate in gates)
        {
            gateDetails.Add(new TimedGateDetails
            {
                Id = position++,
                TimerId = gate.TimerId,
            });
        }

        var domain = new TrackDetails
        {
            Id = Guid.NewGuid(),
            Name = name,
            TimedGates = gateDetails
        };

        _context.Tracks.Add(domain);

        await _context.SaveChangesAsync();

        return new Track(domain.Id, domain.Name, gateDetails.Select(x => x.TimerId));
    }

    public async Task<IEnumerable<Track>> GetAllAsync()
    {
        return await _context.Tracks.Select(x => new Track(x.Id, x.Name, x.TimedGates.Select(x => x.TimerId))).ToListAsync();
    }

    public Task<Track?> GetAsync(Guid id)
    {
        return _context.Tracks.Where(x => x.Id == id).Select(x => new Track(x.Id, x.Name, x.TimedGates.Select(x => x.TimerId))).FirstOrDefaultAsync();
    }

    public async Task<Track?> UpdateAsync(Track update)
    {
        var existing = _context.Tracks.FirstOrDefault(x => x.Id == update.Id);

        if (existing == null)
        {
            return null;
        }

        existing.Name = update.Name;
        int position = -1;
        var updatedGates = new List<TimedGateDetails>();
        foreach (var gate in update.TimedGates)
        {
            updatedGates.Add(new TimedGateDetails
            {
                Id = position++,
                TimerId = gate.TimerId
            });
        }
        existing.TimedGates = updatedGates;

        await _context.SaveChangesAsync();

        return new Track(existing.Id, existing.Name, existing.TimedGates.Select(x => x.TimerId));
    }
}