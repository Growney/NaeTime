using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class PilotQueryHandler : IPilotQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;

    public PilotQueryHandler(IProjectionProvider projectionProvider)
    {
        _projectionProvider = projectionProvider;
    }

    public async Task<IEnumerable<Pilot>> GetAllPilots()
    {
        PilotList _pilotList = await _projectionProvider.Load<PilotList>();
        return _pilotList.GetAllPilots();
    }

    public async Task<Pilot?> GetPilotById(Guid id)
    {
        PilotList _pilotList = await _projectionProvider.Load<PilotList>();
        return _pilotList.GetPilotById(id);
    }
}