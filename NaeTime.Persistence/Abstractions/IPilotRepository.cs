using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface IPilotRepository
{
    public Task<IEnumerable<Pilot>> GetPilots();

    public Task AddOrUpdatePilot(Guid id, string firstName, string lastName, string callSign);
}
