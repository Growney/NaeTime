using NaeTime.Messages.Events.Entities;
using NaeTime.Persistence.Abstractions;

namespace NaeTime.Persistence;
public class PilotService
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
}
