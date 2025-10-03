using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Command;
public class SessionsCommandHandler : ISessionsCommandHandler
{
    private readonly IAggregateRepository _repository;
    private readonly ISessionQueryHandler _sessionsQueryHandler;

    public SessionsCommandHandler(IAggregateRepository repository, ISessionQueryHandler sessionsQueryHandler)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _sessionsQueryHandler = sessionsQueryHandler ?? throw new ArgumentNullException(nameof(sessionsQueryHandler));
    }
    private async Task ThrowIfSessionDoesNotExist(Guid id, Query.Abstractions.Models.SessionType type)
    {
        var session = await _sessionsQueryHandler.GetSession(id);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {id} does not exist.", nameof(id));
        }
        if (session.Type != type)
        {
            throw new ArgumentException($"Session with ID {id} is not of type {type}.", nameof(id));
        }
    }
    public async Task ActivateOpenPracticeSession(Guid id)
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice);

        ActiveSession activeSession = await _repository.Get<ActiveSession, string>()
            ?? _repository.CreateNew<ActiveSession,string>();

        activeSession.ActivateOpenPracticeSession(id);

        await _repository.Save<ActiveSession,string>(activeSession);
    }

    public async Task DeactivateOpenPracticeSession(Guid id)
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice);

        ActiveSession activeSession = await _repository.Get<ActiveSession, string>()
            ?? _repository.CreateNew<ActiveSession, string>();

        activeSession.DeactivateSession(id);

        await _repository.Save<ActiveSession,string>(activeSession);
    }
}
