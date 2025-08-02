using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Projections;

namespace NaeTime.Query;

public class PilotQueryHandler : IPilotQueryHandler
{
    private readonly PilotList _pilotList;

    public PilotQueryHandler(PilotList pilotList)
    {
        _pilotList = pilotList;
    }

    public Task<IEnumerable<Pilot>> GetAllPilots()
    {
        return Task.FromResult(_pilotList.GetAllPilots());
    }

    public Task<Pilot?> GetPilotById(Guid id)
    {
        return Task.FromResult(_pilotList.GetPilotById(id));
    }
}