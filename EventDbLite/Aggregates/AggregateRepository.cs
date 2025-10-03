using EventDbLite.Abstractions;
using EventDbLite.Streams;

namespace EventDbLite.Aggregates;

public class AggregateRepository : IAggregateRepository
{
    private readonly IEventStoreLite _connection;
    private readonly IEventSerializer _eventSerializer;
    private readonly IHandlerProvider _aggregateHandlerProvider;

    public AggregateRepository(IEventStoreLite connection, IEventSerializer eventSerializer, IHandlerProvider aggregateHandlerProvider)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _aggregateHandlerProvider = aggregateHandlerProvider ?? throw new ArgumentNullException(nameof(aggregateHandlerProvider));
    }

    private string GetStreamName<T>(string? id) where T : AggregateRoot
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return typeof(T).Name;
        }
        return $"{typeof(T).Name}-{id}";
    }
    private void Initialize(AggregateRoot aggregateRoot) => aggregateRoot.InitialiseDependencies(_aggregateHandlerProvider, _eventSerializer);

    public async Task<T?> Get<T>(string id) where T : AggregateRoot, new()
    {
        string streamName = GetStreamName<T>(id);

        IAsyncEnumerable<StreamEvent> streamEvents = _connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Beginning);

        T? aggregateRoot = null;

        await foreach (StreamEvent streamEvent in streamEvents)
        {
            if (aggregateRoot == null)
            {
                aggregateRoot = new();
                Initialize(aggregateRoot);
            }

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

    public T CreateNew<T>(Func<T> constructor) where T : AggregateRoot
    {
        T aggregateRoot = constructor();
        Initialize(aggregateRoot);
        return aggregateRoot;
    }
}
