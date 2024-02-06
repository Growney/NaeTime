using NaeTime.Messages.Events.Entities;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub;

namespace NaeTime.Persistence;
public class PilotService : ISubscriber
{
    private readonly IRepositoryFactory _repositoryFactory;

    public PilotService(IRepositoryFactory repositoryFactory)
    {
        _repositoryFactory = repositoryFactory;
    }

    public async Task When(PilotCreated pilot)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository();
        await pilotRepository.AddOrUpdatePilot(pilot.PilotId, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }
    public async Task When(PilotDetailsChanged pilot)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository();
        await pilotRepository.AddOrUpdatePilot(pilot.PilotId, pilot.FirstName, pilot.LastName, pilot.CallSign);
    }

    public async Task<PilotsResponse> On(PilotsRequest request)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository();

        var pilots = await pilotRepository.GetPilots();

        return new PilotsResponse(pilots.Select(x => new PilotsResponse.Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)));
    }
}
