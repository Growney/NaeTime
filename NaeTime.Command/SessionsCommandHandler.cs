using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Command;
public class SessionsCommandHandler(IAggregateRepository repository, ISessionQueryHandler sessionsQueryHandler) : ISessionsCommandHandler
{
    private const string _activeSessionStreamName = "activesession";

    private readonly IAggregateRepository _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    private readonly ISessionQueryHandler _sessionsQueryHandler = sessionsQueryHandler ?? throw new ArgumentNullException(nameof(sessionsQueryHandler));

    private async Task ThrowIfSessionDoesNotExist(Guid id, Query.Abstractions.Models.SessionType type)
    {
        var session = await _sessionsQueryHandler.GetSession(id) ?? throw new ArgumentException($"Session with ID {id} does not exist.", nameof(id));
        if (session.Type != type)
        {
            throw new ArgumentException($"Session with ID {id} is not of type {type}.", nameof(id));
        }
    }
    public async Task ActivateOpenPracticeSession(Guid id)
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(_activeSessionStreamName)
            ?? _repository.CreateNew<ActiveSession>();

        activeSession.ActivateOpenPracticeSession(id);

        await _repository.Save<ActiveSession>(activeSession, _activeSessionStreamName);
    }

    public async Task DeactivateOpenPracticeSession(Guid id)
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(_activeSessionStreamName)
            ?? _repository.CreateNew<ActiveSession>();

        activeSession.DeactivateSession(id);

        await _repository.Save<ActiveSession>(activeSession, _activeSessionStreamName);
    }
}
