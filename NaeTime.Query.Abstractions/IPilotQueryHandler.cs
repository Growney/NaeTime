using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions;
public interface IPilotQueryHandler
{
    public Task<IEnumerable<Pilot>> GetAllPilots();
    public Task<Pilot?> GetPilotById(Guid id);

}
