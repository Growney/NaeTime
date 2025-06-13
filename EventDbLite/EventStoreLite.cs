using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace EventDbLite;

public class EventStoreLite : IEventStoreLite
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, StreamSubscription>> _streamSubscriptions = new();
    private readonly ConcurrentDictionary<Guid, StreamSubscription> _allStreamSubscriptions = new();

    public EventStoreLite(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public async Task AppendToStreamAsync(string streamName, IEnumerable<EventData> data, StreamPosition expectedState)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentException("Stream name cannot be null or empty.", nameof(streamName));
        }

        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        IEnumerable<StreamEvent> storedEvents = await connection.AppendToStreamAsync(streamName, data, expectedState);

        DispatchEvents(streamName, storedEvents);
    }
    public async Task AppendToStreamAsync(string streamName, EventData data, StreamPosition expectedState)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentException("Stream name cannot be null or empty.", nameof(streamName));
        }

        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        StreamEvent storedEvent = await connection.AppendToStreamAsync(streamName, data, expectedState);

        DispatchEvents(streamName, Enumerable.Repeat(storedEvent, 1));
    }

    private void DispatchEvents(string streamName, IEnumerable<StreamEvent> events)
    {
        foreach (StreamEvent streamEvent in events)
        {
            foreach (StreamSubscription subscription in _allStreamSubscriptions.Values)
            {
                subscription.AddLiveEvent(streamEvent);
            }
        }

        if (_streamSubscriptions.TryGetValue(streamName, out var streamSubscriptions))
        {
            foreach (StreamEvent streamEvent in events)
            {
                foreach (StreamSubscription subscription in streamSubscriptions.Values)
                {
                    subscription.AddLiveEvent(streamEvent);
                }
            }
        }
    }
    public async IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, StreamPosition fromPosition)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        await foreach (StreamEvent streamEvent in connection.ReadStreamEvents(streamName, direction, fromPosition))
        {
            yield return streamEvent;
        }
    }
    public async IAsyncEnumerable<StreamEvent> ReadEvents(StreamDirection direction, StreamPosition fromPosition)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        await foreach (StreamEvent streamEvent in connection.ReadAllStreamEvents(direction, fromPosition))
        {
            yield return streamEvent;
        }
    }

    public IStreamSubscription SubscribeToAllStreams(StreamPosition position)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        IAsyncEnumerable<StreamEvent> initialEvents = connection.ReadAllStreamEvents(StreamDirection.Forward, position);

        return CreateSubscription(null, initialEvents);
    }
    public IStreamSubscription SubscribeToStream(string streamName, StreamPosition position)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentException("Stream name cannot be null or empty.", nameof(streamName));
        }

        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStreamConnection connection = scope.ServiceProvider.GetRequiredService<IEventStreamConnection>();

        IAsyncEnumerable<StreamEvent> initialEvents = connection.ReadStreamEvents(streamName, StreamDirection.Forward, position);
        return CreateSubscription(streamName, initialEvents);
    }

    private IStreamSubscription CreateSubscription(string? streamName, IAsyncEnumerable<StreamEvent> initialEvents)
    {
        if (string.IsNullOrEmpty(streamName))
        {
            throw new ArgumentException("Stream name cannot be null or empty.", nameof(streamName));
        }
        Guid subscriptionId = Guid.NewGuid();
        void onDispose(StreamSubscription subscription)
        {
            if (_streamSubscriptions.TryGetValue(streamName, out var subscriptions))
            {
                subscriptions.TryRemove(subscriptionId, out _);
            }
        }

        StreamSubscription subscription = new(initialEvents, onDispose);
        _streamSubscriptions.GetOrAdd(streamName, _ => new ConcurrentDictionary<Guid, StreamSubscription>()).TryAdd(subscriptionId, subscription);
        return subscription;
    }
}
