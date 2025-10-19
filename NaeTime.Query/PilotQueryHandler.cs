using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections.Abstractions;

namespace NaeTime.Query;

public class PilotQueryHandler : IPilotQueryHandler
{
    private readonly IPilotProjection _pilotProjection;

    public PilotQueryHandler(IPilotProjection pilotProjection)
    {
        _pilotProjection = pilotProjection;
    }

    public Task<IEnumerable<Pilot>> GetAllPilots() => Task.FromResult(_pilotProjection.GetAllPilots());

    public Task<Pilot?> GetPilotById(Guid id) => Task.FromResult(_pilotProjection.GetPilotById(id));
}