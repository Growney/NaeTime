using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Streams;

namespace EventDbLite.Projections;

public class Projection
{
    internal IEventSerializer? EventSerializer { get; set; }
    internal IHandlerProvider? HandlerProvider { get; set; }

    internal void Raise(StreamEvent streamEvent)
    {
        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        EventMetadata metadata = EventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);
        if (HandlerProvider is null)
        {
            throw new InvalidOperationException("Handler provider must not be null");
        }

        Handler? handler = HandlerProvider.GetHandlerMethod(this, metadata.Identifier);

        if (handler is null)
        {
            return;
        }

        object? payload = EventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType)
            ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        handler.Action(payload);
    }
}
