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
    private async Task ThrowIfSessionDoesNotExist(Guid id, Events.SessionType type)
    {
        var querySessionType = type switch
        {
            Events.SessionType.OpenPractice => Query.Abstractions.Models.SessionType.OpenPractice,
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"Unsupported session type: {type}")
        };
        var session = await _sessionsQueryHandler.GetSession(id);
        if (session == null)
        {
            throw new ArgumentException($"Session with ID {id} does not exist.", nameof(id));
        }
        if (session.Type != querySessionType)
        {
            throw new ArgumentException($"Session with ID {id} is not of type {type}.", nameof(id));
        }
    }
    public async Task ActivateSession(Guid id, Events.SessionType sessionType)
    {
        await ThrowIfSessionDoesNotExist(id, sessionType);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(ActiveSession.SingletonId)
            ?? _repository.CreateNew(() => new ActiveSession(ActiveSession.SingletonId));

        activeSession.ActivateSession(id, sessionType);

        await _repository.Save(activeSession);
    }

    public async Task DeactivateSession(Guid id, Events.SessionType sessionType)
    {
        await ThrowIfSessionDoesNotExist(id, sessionType);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(ActiveSession.SingletonId)
            ?? _repository.CreateNew(() => new ActiveSession(ActiveSession.SingletonId));

        activeSession.DeactivateSession(id);

        await _repository.Save(activeSession);
    }
}
