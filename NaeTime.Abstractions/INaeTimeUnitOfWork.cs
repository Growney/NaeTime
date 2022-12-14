using NaeTime.Abstractions.Repositories;

namespace NaeTime.Abstractions
{
    public interface INaeTimeUnitOfWork
    {
        INodeRepository Nodes { get; }
        IFlightRepository Flights { get; }
        ITrackRepository Tracks { get; }
        IFlyingSessionRepository FlyingSessions { get; }
        Task SaveChangesAsync();
    }
}
