using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;

namespace EventDbLite.Projections;

public class LiveProjection
{
    internal IEventSerializer? _eventSerializer { get; set; }
    internal IAsyncHandlerProvider? _handlerProvider { get; set; }

    private readonly Dictionary<string, long> _streamPositions = new();
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

        await HandleEvent(streamEvent, metadata);

        GlobalPosition = streamEvent.StreamOrdinal;
        SetStreamPosition(streamEvent.StreamName, streamEvent.StreamOrdinal);
    }

    protected virtual async Task HandleEvent(StreamEvent streamEvent, EventMetadata metadata)
    {
        if (_handlerProvider is null)
        {
            throw new InvalidOperationException("Handler provider must not be null");
        }

        AsyncHandler? handler = _handlerProvider.GetHandlerMethod(this, metadata.Identifier);

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

        await handler.Action(payload);
    }

    public virtual Task<StreamPosition> GetGlobalPosition() => Task.FromResult(StreamPosition.Beginning); // Default implementation, can be overridden in derived classes
}
