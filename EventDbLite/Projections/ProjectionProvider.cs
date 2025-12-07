using EventDbLite.Abstractions;
using EventDbLite.Handlers.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Projections;

public class ProjectionProvider(IServiceProvider serviceProvider, IEventStreamConnection connection, IEventSerializer eventSerializer, IHandlerProvider aggregateHandlerProvider) : IProjectionProvider
{
    private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    private readonly IEventStreamConnection _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    private readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    private readonly IHandlerProvider _handlerProvider = aggregateHandlerProvider ?? throw new ArgumentNullException(nameof(aggregateHandlerProvider));

    public async Task<T> Load<T>(string? streamName = null)
    {
        T projection = ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider);

        IAsyncEnumerable<StreamEvent> streamEvents = (streamName is null)
            ? _connection.ReadAllStreamEvents(StreamDirection.Forward, StreamPosition.Beginning)
            : _connection.ReadStreamEvents(streamName, StreamDirection.Forward, StreamPosition.Beginning);

        await foreach (StreamEvent streamEvent in streamEvents)
        {
            RaiseProjectionEvent(projection, streamEvent);
        }

        return projection;
    }

    private void RaiseProjectionEvent<T>(T projection, StreamEvent streamEvent)
    {
        if (projection == null)
        {
            return;
        }

        EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

        Handlers.Handler? handler = _handlerProvider.GetHandlerMethod(typeof(T), metadata.Identifier);
        if (handler is null)
        {
            return;
        }

        object? payload = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType)
            ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        handler.Action(projection, payload);
    }
}
