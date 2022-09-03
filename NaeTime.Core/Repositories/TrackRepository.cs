using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Repositories;

namespace NaeTime.Core.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        private readonly ApplicationDbContext _context;

        public TrackRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<Track?> GetAsync(Guid id)
           => (from track
               in _context.Tracks
               where track.Id == id
               select track)
            .Include(x => x.Gates)
            .FirstOrDefaultAsync();

        public Task<List<Track>> GetCreatedByPilotAsync(Guid pilot)
           => (from track
               in _context.Tracks
               where track.CreatorPilotId == pilot
               select track)
            .Include(x => x.Gates)
            .ToListAsync();

        public void Insert(Track track) => _context.Tracks.Add(track);

        public void Update(Track track) => _context.Tracks.Update(track);
    }
}
