using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Aggregates;

public class AggregateRepository : IAggregateRepository
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEventStreamConnection _connection;
    private readonly IEventSerializer _eventSerializer;
    private readonly IHandlerProvider _aggregateHandlerProvider;

    public AggregateRepository(IServiceProvider serviceProvider, IEventStreamConnection connection, IEventSerializer eventSerializer, IHandlerProvider aggregateHandlerProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _aggregateHandlerProvider = aggregateHandlerProvider ?? throw new ArgumentNullException(nameof(aggregateHandlerProvider));
    }

    private string GetStreamName<T>(Guid id) where T : AggregateRoot => $"{typeof(T).Name}-{id}";

    public async Task<T> Get<T>(Guid id) where T : AggregateRoot
    {
        T aggregateRoot = ActivatorUtilities.CreateInstance<T>(_serviceProvider);

        aggregateRoot.EventSerializer = _eventSerializer;
        aggregateRoot.HandlerProvider = _aggregateHandlerProvider;

        string streamName = GetStreamName<T>(id);

        IAsyncEnumerable<StreamEvent> streamEvents = _connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Beginning);

        await foreach (StreamEvent streamEvent in streamEvents)
        {
            aggregateRoot.Raise(streamEvent);
        }

        return aggregateRoot;
    }
    public Task Save<T>(T aggregateRoot) where T : AggregateRoot
    {
        IEnumerable<EventData> raisedEvents = aggregateRoot.GetEvents();

        string streamName = GetStreamName<T>(aggregateRoot.Id);

        return _connection.AppendToStreamAsync(streamName, raisedEvents, StreamPosition.WithVersion(aggregateRoot.Version));
    }
}
