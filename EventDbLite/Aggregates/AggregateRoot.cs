using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Handlers;
using EventDbLite.Streams;
using System.Diagnostics.CodeAnalysis;

namespace EventDbLite.Aggregates;

public abstract class AggregateRoot<T> 
{
    public T? Id { get; set; }
    public long Version { get; private set; }

    private IHandlerProvider? _handlerProvider;
    private IEventSerializer? _eventSerializer;

    //Pre initialised events are used to allow the constructor to raise events before the dependencies are set.
    private readonly Queue<object> _preinitialiseEvents = new();

    private readonly List<EventData> _uncommittedEvents = new();
    public IEnumerable<EventData> GetEvents() => _uncommittedEvents;

    internal void InitialiseDependencies(IHandlerProvider handlerProvider, IEventSerializer eventSerializer)
    {
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        RaisePreinitialisedEvents();
    }
    private void RaisePreinitialisedEvents()
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("AggregateRoot is not initialized. Ensure dependencies are set before raising events.");
        }

        while (_preinitialiseEvents.Count > 0)
        {
            object payload = _preinitialiseEvents.Dequeue();
            Raise(payload);
        }
    }

    [MemberNotNullWhen(true, nameof(_handlerProvider), nameof(_eventSerializer))]
    private bool IsInitialized => _handlerProvider is not null && _eventSerializer is not null;

    internal void Raise(StreamEvent streamEvent)
    {
        if (!IsInitialized)
        {
            throw new InvalidOperationException("AggregateRoot is not initialized. Ensure dependencies are set before raising events.");
        }

        EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

        Version = streamEvent.StreamOrdinal;
        Handler? handler = _handlerProvider.GetHandlerMethod(GetType(), metadata.Identifier);
        if (handler is null)
        {
            return;
        }

        object? payload = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, handler.TargetType) ?? throw new InvalidOperationException($"Failed to deserialize event payload for identifier '{metadata.Identifier}'");

        handler.Action(this, payload);
    }
    protected void Raise(object payload)
    {
        if (!IsInitialized)
        {
            _preinitialiseEvents.Enqueue(payload);
            return;
        }

        Version++;
        EventMetadata metadata = _eventSerializer.CreateMetadata(payload);

        byte[] metadataPayload = _eventSerializer.SerializeMetadata(metadata);

        byte[] eventPayload = _eventSerializer.SerializeEvent(payload);

        _uncommittedEvents.Add(new EventData(eventPayload, metadataPayload));

        string identifier = _eventSerializer.GetIdentifier(payload.GetType());

        Handler? handler = _handlerProvider.GetHandlerMethod(GetType(), identifier);

        if (handler is null)
        {
            return;
        }

        handler.Action(this, payload);
    }

    protected void Clone(AggregateRoot<T> root)
    {
        root._handlerProvider = _handlerProvider;
        root._eventSerializer = _eventSerializer;
    }
}
