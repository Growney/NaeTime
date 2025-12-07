using EventDbLite.Abstractions;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace EventDbLite.Reactions;

public class ReactionProvider<TEvent> : IAsyncEnumerable<ReactionEvent<TEvent>>
{
    private readonly IEventStoreLite _store;
    private readonly IEventSerializer _eventSerializer;
    private readonly StreamPosition _initialPosition;
    private readonly IEnumerable<Type> _requirements;
    private readonly ILiveProjectionRepository _repository;
    private readonly string? _streamName;
    private readonly ILogger<ReactionProvider<TEvent>> _logger;

    public ReactionProvider(IEventStoreLite store, IEventSerializer eventSerializer, IEnumerable<Type> requirements, ILiveProjectionRepository repository, StreamPosition initialPosition, ILogger<ReactionProvider<TEvent>> logger, string? streamName)
    {
        _store = store;
        _eventSerializer = eventSerializer;
        _requirements = requirements;
        _repository = repository;
        _initialPosition = initialPosition;
        _logger = logger;
        _streamName = streamName;
    }

    private async Task EnsureRequirementsAsync(long globalVersion, CancellationToken cancellationToken)
    {
        Stopwatch stopwatch = Stopwatch.StartNew();
        List<Task> waitTasks = new();
        foreach (Type requirement in _requirements)
        {
            ILiveProjectionManager? projection = _repository.GetManager(requirement);
            if (projection is not null)
            {
                waitTasks.Add(projection.WaitForVersion(globalVersion, cancellationToken));
            }
        }
        await Task.WhenAll(waitTasks);
        stopwatch.Stop();
        _logger.LogInformation("Waited {ElapsedMilliseconds}ms for requirements at global version {GlobalVersion}", stopwatch.ElapsedMilliseconds, globalVersion);
    }

    public async IAsyncEnumerator<ReactionEvent<TEvent>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        string identifier = _eventSerializer.GetIdentifier(typeof(TEvent));

        IStreamSubscription subscription = _streamName is not null
           ? _store.SubscribeToStream(_streamName, _initialPosition)
           : _store.SubscribeToAllStreams(_initialPosition);

        try
        {
            await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(cancellationToken))
            {
                await EnsureRequirementsAsync(streamEvent.Event.GlobalOrdinal, cancellationToken);

                EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Event.Data.Metadata);

                if (!metadata.Identifier.Equals(identifier))
                {
                    continue;
                }
                object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Event.Data.Payload, typeof(TEvent));

                if (eventObject is TEvent tEvent)
                {
                    yield return new ReactionEvent<TEvent>(tEvent, streamEvent);
                }
            }
        }
        finally
        {
            subscription.Dispose();
        }
    }
}
