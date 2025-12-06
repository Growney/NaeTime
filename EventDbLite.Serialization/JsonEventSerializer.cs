using EventDbLite.Abstractions;
using System.Reflection;

namespace EventDbLite.Serialization;

public class JsonEventSerializer : IEventSerializer
{
    public EventMetadata CreateMetadata(object payload)
    {
        string identifier = GetIdentifier(payload.GetType());

        return new EventMetadata
        {
            Id = Guid.NewGuid(),
            InceptionUtc = DateTime.UtcNow,
            Identifier = identifier
        };
    }
    public object? DeserializeEvent(ReadOnlySpan<byte> payload, Type targetType) => System.Text.Json.JsonSerializer.Deserialize(payload, targetType);
    public EventMetadata DeserializeMetadata(ReadOnlySpan<byte> metadata) => System.Text.Json.JsonSerializer.Deserialize<EventMetadata>(metadata) ?? throw new InvalidOperationException("Failed to deserialize event metadata.");
    public string GetIdentifier(Type eventType)
    {
        EventAttribute? eventAttribute = eventType.GetCustomAttribute<EventAttribute>();

        string identifier = eventAttribute is not null ?
            eventAttribute.Identifier
            : eventType.Name;

        return identifier;
    }
    public byte[] SerializeEvent(object eventData) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(eventData);
    public byte[] SerializeMetadata(EventMetadata metadata) => System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(metadata);
}
