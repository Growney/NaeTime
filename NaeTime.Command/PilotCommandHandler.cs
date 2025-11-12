using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;

namespace NaeTime.Command;
public class PilotCommandHandler(IAggregateRepository repository) : IPilotCommandHandler
{
    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public Task ChangePilotBindingPhrase(Guid id, string bindingPhrase) => ConcurrencyException.Retry(async () =>
    {
        Pilot? pilot = await _repository.Get<Pilot, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        pilot.ChangeBindingPhrase(bindingPhrase);
        await _repository.Save<Pilot, Guid>(pilot).ConfigureAwait(false);
    });

    public Task RemovePilotBindingPhrase(Guid id) => ConcurrencyException.Retry(async () =>
    {
        Pilot? pilot = await _repository.Get<Pilot, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        pilot.RemoveBindingPhrase();
        await _repository.Save<Pilot, Guid>(pilot).ConfigureAwait(false);
    });

    public Task ChangePilotCallsign(Guid id, string? callSign) => ConcurrencyException.Retry(async () =>
    {
        Pilot? pilot = await _repository.Get<Pilot, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        pilot.ChangeCallsign(callSign);
        await _repository.Save<Pilot, Guid>(pilot).ConfigureAwait(false);
    });

    public Task CreatePilot(Guid id, string? firstName, string? lastName, string? callSign, string? bindingPhrase) => ConcurrencyException.Retry(async () =>
    {
        Pilot pilot = _repository.CreateNew<Pilot>(() => new Pilot(id));

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

        await _repository.Save<Pilot, Guid>(pilot).ConfigureAwait(false);
    });

    public Task RenamePilot(Guid id, string? firstName, string? lastName) => ConcurrencyException.Retry(async () =>
    {
        Pilot? pilot = await _repository.Get<Pilot, Guid>(id).ConfigureAwait(false) ?? throw new ArgumentException($"Pilot with ID {id} does not exist.", nameof(id));
        pilot.RenamePilot(firstName, lastName);
        await _repository.Save<Pilot, Guid>(pilot).ConfigureAwait(false);

    });
}
