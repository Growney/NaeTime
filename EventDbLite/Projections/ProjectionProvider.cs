using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

public class ProjectionProvider(IServiceProvider serviceProvider, IEventStreamConnection connection, IEventSerializer eventSerializer, IHandlerProvider aggregateHandlerProvider) : IProjectionProvider
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IEventStreamConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    private readonly IHandlerProvider _handlerProvider = aggregateHandlerProvider ?? throw new ArgumentNullException(nameof(aggregateHandlerProvider));

    public async Task<T> Load<T>(string? streamName = null) where T : Projection
    {
        T projection = ActivatorUtilities.CreateInstance<T>(_serviceProvider);

        projection.EventSerializer = _eventSerializer;
        projection.HandlerProvider = _handlerProvider;

        IAsyncEnumerable<StreamEvent> streamEvents = (streamName is null)
            ? _connection.ReadAllStreamEvents(StreamDirection.Forward, StreamPosition.Beginning)
            : _connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Beginning);

        await foreach (StreamEvent streamEvent in streamEvents)
        {
            projection.Raise(streamEvent);
        }

        return projection;
    }

    public async Task Refresh<T>(T projection) where T : Projection
    {
        projection.EventSerializer = _eventSerializer;
        projection.HandlerProvider = _handlerProvider;

        IAsyncEnumerable<StreamEvent> streamEvents = (projection.StreamName is null)
            ? _connection.ReadAllStreamEvents(StreamDirection.Forward, StreamPosition.WithVersion(projection.GlobalOrdinal))
            : _connection.ReadStreamEvents(projection.StreamName, StreamDirection.Forward, StreamPosition.WithVersion(projection.GlobalOrdinal));

        await foreach (StreamEvent streamEvent in streamEvents)
        {
            projection.Raise(streamEvent);
        }
    }
}
