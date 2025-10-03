using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;

namespace EventDbLite.Projections;

public class Projection
{
    internal IEventSerializer? EventSerializer { get; set; }
    internal IHandlerProvider? HandlerProvider { get; set; }
    internal string? StreamName { get; set; }
    public long GlobalOrdinal { get; private set; }

    internal void Raise(StreamEvent streamEvent)
    {
        if (streamEvent.GlobalOrdinal <= GlobalOrdinal)
        {
            return; // Ignore events that are not newer than the current state
        }

        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        EventMetadata metadata = EventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
        if (HandlerProvider is null)
        {
            throw new InvalidOperationException("Handler provider must not be null");
        }

        Handler? handler = HandlerProvider.GetHandlerMethod(this.GetType(), metadata.Identifier);

        if (handler is null)
        {
            return;
        }

        object? payload = EventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType)
            ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        handler.Action(this, payload);

        GlobalOrdinal = streamEvent.GlobalOrdinal;
    }

    protected virtual void HandleEvent(StreamEvent streamEvent, EventMetadata metadata)
    {
        // Override in derived classes to handle events
    }
}
