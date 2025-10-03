using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Streams;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EventDbLite.Reactions;

public class ReactionProvider : IReactionProvider
{
    private class ReactionHandler : IDisposable
    {
        public ReactionHandler(Func<object, StreamEvent, Task> handler, Action onDispose)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            OnDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public Func<object, StreamEvent, Task> Handler { get; }
        public Action OnDispose { get; }

        public void Dispose()
        {
            OnDispose?.Invoke();
        }
    }
    private class EventBuffer
    {
        private readonly CancellationTokenSource _cancellationSource = new();
        private readonly SemaphoreSlim _signal = new(0);
        private readonly ConcurrentQueue<object> _queue = new();

        public void AddItem(object item)
        {
            _queue.Enqueue(item);
        }

        public async Task<object?> WaitForItemAsync(CancellationToken cancellationToken = default)
        {
            object? item = null;
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
        await foreach (StreamEvent streamEvent in _subscription.StreamEvents(_cancellationTokenSource.Token))
        {
            EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
            Task handlerProcess = ProcessHandlers(streamEvent, metadata, streamEvent.Data.Payload);

            ProcessAwaiters(metadata, streamEvent.Data.Payload);

            await handlerProcess;
        }
    }

    private void ProcessAwaiters(EventMetadata metadata, byte[] data)
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
                    bufferKvp.Value.AddItem(eventData);
                }
            }
        }
    }
    private Task ProcessHandlers(StreamEvent steamEvent, EventMetadata metadata, byte[] data)
    {
        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        List<Task> tasks = new();
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
                    tasks.Add(handler.Value.Handler(deserializedData, steamEvent));
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
    public IDisposable On(Type type, Func<object, StreamEvent, Task> handler)
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
    public async IAsyncEnumerable<object> StreamEvents(Type type, [EnumeratorCancellation] CancellationToken cancellationToken)
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
                object? item = await waiter.WaitForItemAsync(cancellationToken);
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
