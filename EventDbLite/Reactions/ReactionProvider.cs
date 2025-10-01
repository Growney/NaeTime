using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Projections;
using EventDbLite.Streams;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace EventDbLite.Reactions;

public class ReactionProvider : LiveProjection, IReactionProvider, IDisposable
{
    private class ReactionHandler : IDisposable
    {
        public ReactionHandler(Func<object, Task> handler, Action onDispose)
        {
            Handler = handler ?? throw new ArgumentNullException(nameof(handler));
            OnDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
        }

        public Func<object, Task> Handler { get; }
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

    protected override async Task HandleEvent(StreamEvent streamEvent, EventMetadata metadata)
    {
        Task handlerProcess = ProcessHandlers(metadata, streamEvent.Data.Payload);

        ProcessAwaiters(metadata, streamEvent.Data.Payload);

        await handlerProcess;
    }

    private void ProcessAwaiters(EventMetadata metadata, byte[] data)
    {
        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        if (_waiters.TryGetValue(metadata.Identifier, out var waiters))
        {
            foreach (var waiter in waiters)
            {
                object? eventData = EventSerializer.DeserializeEvent(data, waiter.Key);

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
    private Task ProcessHandlers(EventMetadata metadata, byte[] data)
    {
        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        List<Task> tasks = new();
        if (_handlers.TryGetValue(metadata.Identifier, out var handlers))
        {
            foreach (KeyValuePair<Type, ConcurrentDictionary<Guid, ReactionHandler>> handlerKvp in handlers)
            {
                object? deserializedData = EventSerializer.DeserializeEvent(data, handlerKvp.Key);

                if (deserializedData is null)
                {
                    continue; // No data to process
                }

                foreach (KeyValuePair<Guid, ReactionHandler> handler in handlerKvp.Value)
                {

                    tasks.Add(handler.Value.Handler(deserializedData));
                }
            }
        }
        return Task.WhenAll(tasks);
    }

    public void Dispose()
    {
        _handlers.Clear();
        _waiters.Clear();
    }

    public IDisposable On<T>(Func<T, Task> handler)
    {

        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        Guid handlerId = Guid.NewGuid();

        Type targetType = typeof(T);

        string identifier = EventSerializer.GetIdentifier(targetType);

        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ReactionHandler>> handlerBag = _handlers.GetOrAdd(identifier, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, ReactionHandler>>());
        ConcurrentDictionary<Guid, ReactionHandler> handlers = handlerBag.GetOrAdd(targetType, _ => new ConcurrentDictionary<Guid, ReactionHandler>());

        Task Handler(object obj)
        {
            if (obj is T typedObj)
            {
                return handler(typedObj);
            }
            return Task.CompletedTask;
        }
        void OnDispose()
        {
            handlers.TryRemove(handlerId, out _);
        }
        ReactionHandler reactionHandler = new(Handler, OnDispose);
        handlers.TryAdd(handlerId, reactionHandler);

        return reactionHandler;
    }

    public async IAsyncEnumerable<T> GetEvents<T>([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        Type targetType = typeof(T);
        string identifier = EventSerializer.GetIdentifier(typeof(T));

        var waiter = new EventBuffer();
        Guid waiterId = Guid.NewGuid();
        ConcurrentDictionary<Type, ConcurrentDictionary<Guid, EventBuffer>> waiterBag = _waiters.GetOrAdd(identifier, _ => new ConcurrentDictionary<Type, ConcurrentDictionary<Guid, EventBuffer>>());
        ConcurrentDictionary<Guid, EventBuffer> buffers = waiterBag.GetOrAdd(targetType, _ => new ConcurrentDictionary<Guid, EventBuffer>());
        buffers.TryAdd(waiterId, waiter);

        try
        {

            while (!cancellationToken.IsCancellationRequested)
            {
                object? item = await waiter.WaitForItemAsync(cancellationToken);
                if (item is T typedItem)
                {
                    yield return typedItem;
                }
            }
        }
        finally
        {
            buffers.TryRemove(waiterId, out _);
        }
    }
}
