namespace NaeTime.Command.Abstractions;
public interface IPilotCommandHandler
{
    public Task CreatePilot(Guid id, string? firstName, string? lastName, string? callSign, string? bindingPhrase);
    public Task RenamePilot(Guid id, string? firstName, string? lastName);
    public Task ChangePilotCallsign(Guid id, string? callSign);
    public Task ChangePilotBindingPhrase(Guid id, string bindingPhrase);
    public Task RemovePilotBindingPhrase(Guid id);
}
