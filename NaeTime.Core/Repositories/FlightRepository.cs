using Microsoft.EntityFrameworkCore;
using NaeTime.Abstractions.Models;
using NaeTime.Abstractions.Repositories;

namespace NaeTime.Core.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly ApplicationDbContext _context;

        public FlightRepository(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<Flight?> GetAsync(Guid id)
            => (from flight in _context.Flights
                where flight.Id == id
                select flight)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Readings)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Passes)
            .FirstOrDefaultAsync();

        public Task<List<Flight>> GetForPilotAsync(Guid pilotId)
            => (from flight in _context.Flights
                where flight.PilotId == pilotId
                select flight)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Readings)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Passes)
            .ToListAsync();

        public Task<Flight?> GetForStreamAsync(Guid streamId)
            => (from flight in _context.Flights
                from stream in flight.RssiStreams
                where stream.Id == streamId
                select flight)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Passes)
            .FirstOrDefaultAsync();

        public Task<List<Flight>> GetForTrackAsync(Guid trackId)
            => (from flight in _context.Flights
                where flight.TrackId == trackId
                select flight)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Readings)
            .Include(x => x.RssiStreams)
                .ThenInclude(s => s.Passes)
            .ToListAsync();
    }
}
