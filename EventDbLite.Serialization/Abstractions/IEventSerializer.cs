namespace EventDbLite.Abstractions;

public interface IEventSerializer
{
    object? DeserializeEvent(ReadOnlySpan<byte> payload, Type targetType);
    byte[] SerializeEvent(object eventData);

    EventMetadata DeserializeMetadata(ReadOnlySpan<byte> metadata);
    byte[] SerializeMetadata(EventMetadata metadata);

    EventMetadata CreateMetadata(object payload);
    string GetIdentifier(Type eventType);
}