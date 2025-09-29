namespace NaeTime.Command.Abstractions;
public interface ISessionsCommandHandler
{
    public Task ActivateSession(Guid id);
    public Task DeactivateSession(Guid id);
}
