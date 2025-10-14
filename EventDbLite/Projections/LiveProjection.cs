using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;

namespace EventDbLite.Projections;

public class LiveProjection(IEventSerializer eventSerializer, IAsyncHandlerProvider handlerProvider)
{
    protected readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    protected readonly IAsyncHandlerProvider _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));

    private readonly Dictionary<string, long> _streamPositions = [];

    public long GlobalPosition { get; private set; }

    public long GetStreamPosition(string streamName)
    {
        if (_streamPositions.TryGetValue(streamName, out long position))
        {
            return position;
        }

        return StreamPosition.Beginning.Version; // Default to start if not found
    }
    private void SetStreamPosition(string streamName, long position)
    {
        if (position < StreamPosition.Beginning.Version)
        {
            throw new ArgumentOutOfRangeException(nameof(position), "Position must be greater than or equal to Start.");
        }

        _streamPositions[streamName] = position;
    }
    internal async Task Raise(StreamEvent streamEvent)
    {
        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

        GlobalPosition = streamEvent.StreamOrdinal;
        SetStreamPosition(streamEvent.StreamName, streamEvent.StreamOrdinal);

        await HandleEvent(streamEvent, metadata);
    }

    protected virtual async Task HandleEvent(StreamEvent streamEvent, EventMetadata metadata)
    {
        if (_handlerProvider is null)
        {
            throw new InvalidOperationException("Handler provider must not be null");
        }

        AsyncHandler? handler = _handlerProvider.GetHandlerMethod(GetType(), metadata.Identifier);

        if (handler is null)
        {
            return;
        }

        if (_eventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        object? payload = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType)
            ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        await handler.Action(this, payload);
    }

    public virtual Task<StreamPosition> GetGlobalPosition() => Task.FromResult(StreamPosition.End); // Default implementation, can be overridden in derived classes
}
