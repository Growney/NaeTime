using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class PilotCommandHandler : IPilotCommandHandler
{
    private readonly IAggregateRepository _repository;

    public PilotCommandHandler(IAggregateRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task ChangePilotBindingPhrase(Guid id, string bindingPhrase)
    {
        Pilot? pilot = await _repository.Get<Pilot>(id);
        if (pilot == null)
        {
            throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        }
        pilot.ChangeBindingPhrase(bindingPhrase);
        await _repository.Save(pilot);
    }

    public async Task RemovePilotBindingPhrase(Guid id)
    {
        Pilot? pilot = await _repository.Get<Pilot>(id);
        if (pilot == null)
        {
            throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        }
        pilot.RemoveBindingPhrase();
        await _repository.Save(pilot);
    }
    public async Task ChangePilotCallsign(Guid id, string? callSign)
    {
        Pilot? pilot = await _repository.Get<Pilot>(id);
        if (pilot == null)
        {
            throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        }
        pilot.ChangeCallsign(callSign);
        await _repository.Save(pilot);
    }

    public async Task CreatePilot(Guid id, string? firstName, string? lastName, string? callSign, string? bindingPhrase)
    {
        Pilot pilot = new(id);

        if (!string.IsNullOrEmpty(firstName) || !string.IsNullOrEmpty(lastName))
        {
            pilot.RenamePilot(firstName, lastName);
        }
        if (!string.IsNullOrEmpty(callSign))
        {
            pilot.ChangeCallsign(callSign);
        }
        if (!string.IsNullOrEmpty(bindingPhrase))
        {
            pilot.ChangeBindingPhrase(bindingPhrase);
        }

        await _repository.Save(pilot);
    }

    public async Task RenamePilot(Guid id, string? firstName, string? lastName)
    {
        Pilot? pilot = await _repository.Get<Pilot>(id);
        if (pilot == null)
        {
            throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        }
        pilot.RenamePilot(firstName, lastName);
        await _repository.Save(pilot);

    }
}
