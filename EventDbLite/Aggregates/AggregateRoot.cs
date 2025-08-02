using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;

namespace EventDbLite.Aggregates;

public abstract class AggregateRoot
{
    public Guid Id { get; set; }
    public long Version { get; private set; }

    internal IHandlerProvider? HandlerProvider { get; set; }
    internal IEventSerializer? EventSerializer { get; set; }

    private readonly List<EventData> _uncommittedEvents = new();
    public IEnumerable<EventData> GetEvents() => _uncommittedEvents;

    internal void Raise(StreamEvent streamEvent)
    {
        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        EventMetadata metadata = EventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

        if (HandlerProvider is null)
        {
            throw new InvalidOperationException("Method provider must not be null");
        }

        Handler? handler = HandlerProvider.GetHandlerMethod(this, metadata.Identifier);
        if (handler is null)
        {
            return;
        }

        object? payload = EventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType) ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        handler.Action(payload);

        Version = streamEvent.StreamOrdinal;
    }

    protected void Raise(object payload)
    {
        if (HandlerProvider is null)
        {
            throw new InvalidOperationException("Method provider must not be null");
        }

        if (EventSerializer is null)
        {
            throw new InvalidOperationException("Event serializer must not be null");
        }

        string identifier = EventSerializer.GetIdentifier(payload.GetType());

        Handler? handler = HandlerProvider.GetHandlerMethod(this, identifier);

        if (handler is null)
        {
            return;
        }

        handler.Action(payload);

        EventMetadata metadata = EventSerializer.CreateMetadata(payload);

        byte[] metadataPayload = EventSerializer.SerializeMetadata(metadata);

        byte[] eventPayload = EventSerializer.SerializeEvent(payload);

        _uncommittedEvents.Add(new EventData(metadataPayload, eventPayload));

        Version++;
    }
}
