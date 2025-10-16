using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Streams;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EventDbLite.Reactions;

public class ReactionProvider : IReactionProvider
{
    private class ReactionHandler(Func<ReactionEvent, Task> handler, Action onDispose) : IDisposable
    {
        public Func<ReactionEvent, Task> Handler { get; } = handler ?? throw new ArgumentNullException(nameof(handler));
        public Action OnDispose { get; } = onDispose ?? throw new ArgumentNullException(nameof(onDispose));

        public void Dispose()
        {
            OnDispose?.Invoke();
        }
    }
    private class EventBuffer
    {
        private readonly CancellationTokenSource _cancellationSource = new();
        private readonly SemaphoreSlim _signal = new(0);
        private readonly ConcurrentQueue<ReactionEvent> _queue = new();

        public void AddItem(ReactionEvent item)
        {
            _queue.Enqueue(item);
        }

        public async Task<ReactionEvent?> WaitForItemAsync(CancellationToken cancellationToken = default)
        {
            ReactionEvent? item = null;
            while (!_queue.TryDequeue(out item) && !_cancellationSource.IsCancellationRequested)
            {
                await _signal.WaitAsync(1000, cancellationToken);
            }

            return item;
        }
    }

    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ReactionHandler>>> _handlers = new();
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Type, ConcurrentDictionary<Guid, EventBuffer>>> _waiters = new();

    private readonly IStreamSubscription _subscription;
    protected readonly IEventSerializer _eventSerializer;

    private Task _processTask;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ReactionProvider(IStreamSubscription subscription, IEventSerializer eventSerializer)
    {
        _subscription = subscription ?? throw new ArgumentNullException(nameof(subscription));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));

        _processTask = ProcessEvents();
    }

    protected async Task ProcessEvents()
    {
        await foreach (SubscriptionEvent streamEvent in _subscription.StreamEvents(_cancellationTokenSource.Token))
        {
            EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Event.Data.Metadata);
            Task handlerProcess = ProcessHandlers(streamEvent, metadata, streamEvent.Event.Data.Payload);

            ProcessAwaiters(streamEvent, metadata, streamEvent.Event.Data.Payload);

            await handlerProcess;
        }
    }

    private void ProcessAwaiters(SubscriptionEvent subscriptionEvent, EventMetadata metadata, byte[] data)
    {
        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        if (_waiters.TryGetValue(metadata.Identifier, out var waiters))
        {
            foreach (var waiter in waiters)
            {
                object? eventData = _eventSerializer.DeserializeEvent(data, waiter.Key);

                if (eventData is null)
                {
                    continue; // No data to process
                }

                foreach (KeyValuePair<Guid, EventBuffer> bufferKvp in waiter.Value)
                {
                    bufferKvp.Value.AddItem(new ReactionEvent(eventData, subscriptionEvent));
                }
            }
        }
    }
    private Task ProcessHandlers(SubscriptionEvent subscriptionEvent, EventMetadata metadata, byte[] data)
    {
        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        List<Task> tasks = [];
        if (_handlers.TryGetValue(metadata.Identifier, out var handlers))
        {
            foreach (KeyValuePair<Type, ConcurrentDictionary<Guid, ReactionHandler>> handlerKvp in handlers)
            {
                object? deserializedData = _eventSerializer.DeserializeEvent(data, handlerKvp.Key);

                if (deserializedData is null)
                {
                    continue; // No data to process
                }

                foreach (KeyValuePair<Guid, ReactionHandler> handler in handlerKvp.Value)
                {
                    tasks.Add(handler.Value.Handler(new ReactionEvent(deserializedData, subscriptionEvent)));
                }
            }
        }
        return Task.WhenAll(tasks);
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationTokenSource.Cancel();

        await _processTask;

        _handlers.Clear();
        _waiters.Clear();
    }
    public IDisposable On(Type type, Func<ReactionEvent, Task> handler)
    {

        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        Guid handlerId = Guid.NewGuid();

        string identifier = _eventSerializer.GetIdentifier(type);

        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ReactionHandler>> handlerBag = _handlers.GetOrAdd(identifier, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ReactionHandler>>());
        ConcurrentDictionary<Guid, ReactionHandler> handlers = handlerBag.GetOrAdd(type, _ => new ConcurrentDictionary<Guid, ReactionHandler>());

        void OnDispose()
        {
            handlers.TryRemove(handlerId, out _);
        }
        ReactionHandler reactionHandler = new(handler, OnDispose);
        handlers.TryAdd(handlerId, reactionHandler);

        return reactionHandler;
    }
    public async IAsyncEnumerable<ReactionEvent> StreamSubscription(Type type, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        string identifier = _eventSerializer.GetIdentifier(type);

        var waiter = new EventBuffer();
        Guid waiterId = Guid.NewGuid();
        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, EventBuffer>> waiterBag = _waiters.GetOrAdd(identifier, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, EventBuffer>>());
        ConcurrentDictionary<Guid, EventBuffer> buffers = waiterBag.GetOrAdd(type, _ => new ConcurrentDictionary<Guid, EventBuffer>());
        buffers.TryAdd(waiterId, waiter);

        try
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                ReactionEvent? item = await waiter.WaitForItemAsync(cancellationToken);
                if (item != null)
                {
                    yield return item;
                }
            }
        }
        finally
        {
            buffers.TryRemove(waiterId, out _);
        }
    }
}
