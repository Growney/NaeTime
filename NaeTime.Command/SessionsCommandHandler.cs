using EventDbLite.Abstractions;
using EventDbLite.Exceptions;
using NaeTime.Command.Abstractions;
using NaeTime.Command.Aggregates;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Command;
public class SessionsCommandHandler : ISessionsCommandHandler
{
    private const string _activeSessionStreamName = "activesession";

    private readonly IAggregateRepository _repository;
    private readonly IProjectionProvider _projectionProvider;

    public SessionsCommandHandler(IAggregateRepository repository, IProjectionProvider projectionProvider)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _projectionProvider = projectionProvider ?? throw new ArgumentNullException(nameof(projectionProvider));
    }

    private Task ThrowIfSessionDoesNotExist(Guid id, Query.Abstractions.Models.SessionType type) => ConcurrencyException.Retry(async () =>
    {
        ISessionQueryHandler queryHandler = await _projectionProvider.Load<ISessionQueryHandler>().ConfigureAwait(false);
        Session? session = await queryHandler.GetSession(id);
        if (session == null || session.Type != type)
        {
            throw new ArgumentException($"Session with ID {id} is not of type {type}.", nameof(id));
        }
    });
    public Task ActivateOpenPracticeSession(Guid id) => ConcurrencyException.Retry(async () =>
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice).ConfigureAwait(false);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(_activeSessionStreamName).ConfigureAwait(false)
            ?? _repository.CreateNew<ActiveSession>();

        activeSession.ActivateOpenPracticeSession(id);

        await _repository.Save<ActiveSession>(activeSession, _activeSessionStreamName).ConfigureAwait(false);
    });

    public Task DeactivateOpenPracticeSession(Guid id) => ConcurrencyException.Retry(async () =>
    {
        await ThrowIfSessionDoesNotExist(id, SessionType.OpenPractice).ConfigureAwait(false);

        ActiveSession activeSession = await _repository.Get<ActiveSession>(_activeSessionStreamName).ConfigureAwait(false)
            ?? _repository.CreateNew<ActiveSession>();

        activeSession.DeactivateSession(id);

        await _repository.Save<ActiveSession>(activeSession, _activeSessionStreamName).ConfigureAwait(false);
    });
}
