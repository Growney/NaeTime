using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;

namespace EventDbLite.Reactions;
public class ReactionClassContainer<T> : IDisposable
{
    private readonly IEventStoreLite _store;
    private readonly IAsyncHandlerProvider _handlerProvider;
    private readonly IEventSerializer _eventSerializer;

    private readonly CancellationTokenSource _cts = new();

    public T Instance { get; }
    public ReactionClassContainer(T instance, IAsyncHandlerProvider handlerProvider, IEventStoreLite store, IEventSerializer eventSerializer)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
        _eventSerializer = eventSerializer;

        _ = ProcessClass(Instance);
    }

    private async Task ProcessClass(T instance)
    {
        if (instance == null)
        {
            return;
        }

        IEnumerable<AsyncHandler> handlers = _handlerProvider.GetHandlerMethods(typeof(T));

        Dictionary<string, AsyncHandler> handlerMap = handlers.ToDictionary(h => _eventSerializer.GetIdentifier(h.TargetType));

        IStreamSubscription subscription = _store.SubscribeToAllStreams(StreamPosition.End);

        try
        {
            await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(_cts.Token))
            {
                EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Event.Data.Metadata);

                if (!handlerMap.TryGetValue(metadata.Identifier, out var handler))
                {
                    continue;
                }

                object? eventObject = _eventSerializer.DeserializeEvent(streamEvent.Event.Data.Payload, handler.TargetType);

                if (eventObject is null)
                {
                    continue;
                }

                await handler.Action(instance, eventObject);
            }
        }
        finally
        {
            subscription.Dispose();
        }

    }

    public void Dispose() => _cts.Cancel();
}
