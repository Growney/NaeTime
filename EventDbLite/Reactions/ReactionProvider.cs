using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Projections;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Streams;

namespace EventDbLite.Reactions;

public class ReactionProvider<TEvent> : IReactionProvider<TEvent>
{
    private readonly IEventStoreLite _store;
    private readonly IEventSerializer _eventSerializer;
    private readonly StreamPosition _initialPosition;
    private readonly IEnumerable<Type> _requirements;
    private readonly ILiveProjectionRepository _repository;
    private readonly string? _streamName;
    private CancellationTokenSource _source;

    public ReactionProvider(IEventStoreLite store, IEventSerializer eventSerializer,IEnumerable<Type> requirements,ILiveProjectionRepository repository, StreamPosition initialPosition, string? streamName, CancellationToken mainToken)
    {
        _store = store;
        _eventSerializer = eventSerializer;
        _requirements = requirements;
        _repository = repository;
        _initialPosition = initialPosition;
        _streamName = streamName;
        _source = CancellationTokenSource.CreateLinkedTokenSource(mainToken);
    }

    private Task EnsureRequirementsAsync(long globalVersion,CancellationToken cancellationToken)
    {
        List<Task> waitTasks = new();
        foreach (Type requirement in _requirements)
        {
            ILiveProjectionManager? projection = _repository.GetManager(requirement);
            if (projection is not null)
            {
                waitTasks.Add(projection.WaitForVersion(globalVersion,cancellationToken));
            }
        }
        return Task.WhenAll(waitTasks);
    }

    public async IAsyncEnumerator<ReactionEvent<TEvent>> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        _source = CancellationTokenSource.CreateLinkedTokenSource(_source.Token, cancellationToken);

        string identifier = _eventSerializer.GetIdentifier(typeof(TEvent));

        IStreamSubscription subscription = _streamName is not null
           ? _store.SubscribeToStream(_streamName, _initialPosition)
           : _store.SubscribeToAllStreams(_initialPosition);

        try
        {
            await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(cancellationToken))
            {
                await EnsureRequirementsAsync(streamEvent.Event.GlobalOrdinal, _source.Token);

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
