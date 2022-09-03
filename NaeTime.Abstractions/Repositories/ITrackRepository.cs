using NaeTime.Abstractions.Models;

namespace NaeTime.Abstractions.Repositories
{
    public interface ITrackRepository
    {
        Task<Track?> GetAsync(Guid id);
        Task<List<Track>> GetCreatedByPilotAsync(Guid pilot);

        void Insert(Track track);
        void Update(Track track);
    }
}
