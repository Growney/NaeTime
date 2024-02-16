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
        var pilotRepository = await _repositoryFactory.CreatePilotRepository().ConfigureAwait(false);
        await pilotRepository.AddOrUpdatePilot(pilot.PilotId, pilot.FirstName, pilot.LastName, pilot.CallSign).ConfigureAwait(false);
    }
    public async Task When(PilotDetailsChanged pilot)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository().ConfigureAwait(false);
        await pilotRepository.AddOrUpdatePilot(pilot.PilotId, pilot.FirstName, pilot.LastName, pilot.CallSign).ConfigureAwait(false);
    }

    public async Task<PilotsResponse> On(PilotsRequest request)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository().ConfigureAwait(false);

        var pilots = await pilotRepository.GetPilots().ConfigureAwait(false);

        return new PilotsResponse(pilots.Select(x => new PilotsResponse.Pilot(x.Id, x.FirstName, x.LastName, x.CallSign)));
    }

    public async Task<PilotResponse?> On(PilotRequest request)
    {
        var pilotRepository = await _repositoryFactory.CreatePilotRepository().ConfigureAwait(false);

        var pilot = await pilotRepository.GetPilot(request.PilotId).ConfigureAwait(false);

        return pilot == null ? null : new PilotResponse(pilot.Id, pilot.FirstName, pilot.LastName, pilot.CallSign);

    }
}
