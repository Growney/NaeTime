using NaeTime.Abstractions;
using NaeTime.Abstractions.Repositories;

namespace NaeTime.Core
{
    public class NaeTimeUnitOfWork : INaeTimeUnitOfWork
    {
        public ITrackRepository Tracks { get; }
        public IFlightRepository Flights { get; }
        public INodeRepository Nodes { get; }
        public IFlyingSessionRepository FlyingSessions { get; }

        private readonly ApplicationDbContext _context;

        public NaeTimeUnitOfWork(ApplicationDbContext context,
            INodeRepository nodeRepository,
            ITrackRepository tracks,
            IFlightRepository flights,
            IFlyingSessionRepository flyingSessions)
        {
            _context = context;
            Nodes = nodeRepository;
            Tracks = tracks;
            Flights = flights;
            FlyingSessions = flyingSessions;
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
