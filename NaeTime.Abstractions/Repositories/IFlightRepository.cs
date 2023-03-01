using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Repositories
{
    public interface IFlightRepository
    {
        Task<Flight?> GetAsync(Guid id);
        Task<Flight?> GetWithReadings(Guid id);
        Task<List<Flight>> GetForPilotAsync(Guid pilotId);
        Task<List<Flight>> GetForTrackAsync(Guid trackId);
        Task<Flight?> GetForStreamAsync(Guid streamId);
        Task<List<Flight>> GetActiveAsync();
    }
}
