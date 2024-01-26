using Microsoft.EntityFrameworkCore;
using NaeTime.Client.Razor.Lib.Abstractions;
using NaeTime.Client.Razor.Lib.Models;
using NaeTime.Client.Razor.Lib.SQlite.Models;

namespace NaeTime.Client.Razor.Lib.SQlite;
internal class FlyingSessionApiClient : IFlyingSessionApiClient
{
    private readonly NaeTimeDbContext _context;

    public FlyingSessionApiClient(NaeTimeDbContext context)
    {
        _context = context;
    }
    public async Task<FlyingSession?> CreateAsync(string description, DateTime start, DateTime expectedEnd, Guid trackId)
    {
        var domain = new FlyingSessionDetails
        {
            Id = Guid.NewGuid(),
            Description = description,
            Start = start,
            ExpectedEnd = expectedEnd,
            TrackId = trackId
        };

        _context.FlyingSessions.Add(domain);

        await _context.SaveChangesAsync();

        return new FlyingSession(domain.Id, domain.Description, domain.Start, domain.ExpectedEnd, domain.TrackId);
    }

    public async Task<IEnumerable<FlyingSession>> GetAllAsync()
    {
        return await _context.FlyingSessions.Select(x => new FlyingSession(x.Id, x.Description, x.Start, x.ExpectedEnd, x.TrackId)).ToListAsync();
    }

    public Task<FlyingSession?> GetAsync(Guid id)
    {
        return _context.FlyingSessions.Where(x => x.Id == id).Select(x => new FlyingSession(x.Id, x.Description, x.Start, x.ExpectedEnd, x.TrackId)).FirstOrDefaultAsync();
    }

    public async Task<FlyingSession?> UpdateAsync(FlyingSession flyingSession)
    {
        var existing = _context.FlyingSessions.FirstOrDefault(x => x.Id == flyingSession.Id);

        if (existing == null)
        {
            return null;
        }

        existing.Description = flyingSession.Description;
        existing.Start = flyingSession.Start;
        existing.ExpectedEnd = flyingSession.ExpectedEnd;
        existing.TrackId = flyingSession.TrackId;

        await _context.SaveChangesAsync();

        return new FlyingSession(existing.Id, existing.Description, existing.Start, existing.ExpectedEnd, existing.TrackId);
    }
}