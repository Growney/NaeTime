namespace NaeTime.Command.Abstractions;
public interface ISessionsCommandHandler
{
    public Task ActivateOpenPracticeSession(Guid id);
    public Task DeactivateOpenPracticeSession(Guid id);
}
