using EventSourcingCore.Event.Abstractions.Metadata;
using System;

namespace EventSourcingCore.Event.Abstractions
{
    public interface IEventFactory
    {

        bool TryCreateEvent<TEvent>(ReadOnlyMemory<byte> data, out TEvent eventObj) where TEvent : IEvent;
        bool TryCreateEvent<TEvent>(byte[] data, out TEvent eventObj) where TEvent : IEvent;

        bool TryCreateEvent(Type eventType, byte[] data, out IEvent eventObj);
        bool TryCreateEvent(Type eventType, ReadOnlyMemory<byte> data, out IEvent eventObj);

        bool TryCreateMetadata<T>(byte[] data, out T metadata) where T : IEventMetadata;
        bool TryCreateMetadata<T>(ReadOnlyMemory<byte> data, out T metadata) where T : IEventMetadata;

        bool TryCreateMetadata(Type metadataType, byte[] data, out IEventMetadata metadata);
        bool TryCreateMetadata(Type metadataType, ReadOnlyMemory<byte> data, out IEventMetadata metadata);
        EventData CreateData(IEvent eventObj, IEventMetadata metadata);
    }
}
