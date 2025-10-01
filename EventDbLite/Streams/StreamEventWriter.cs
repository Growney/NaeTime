using EventDbLite.Abstractions;
using EventDbLite.Events;

namespace EventDbLite.Streams;
public class StreamEventWriter : IStreamEventWriter
{
    private readonly IEventSerializer _eventSerializer;
    private readonly IEventStreamConnection _connection;
    public StreamEventWriter(IEventSerializer eventSerializer, IEventStreamConnection connection)
    {
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
    }
    public async Task AppendToStream(string streamName, object payload)
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name must not be null or whitespace.", nameof(streamName));
        }
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }
        EventMetadata metaData = _eventSerializer.CreateMetadata(payload);

        byte[] metadataPayload = _eventSerializer.SerializeMetadata(metaData);
        byte[] eventPayload = _eventSerializer.SerializeEvent(payload);

        EventData data = new(eventPayload, metadataPayload);

        string identifier = _eventSerializer.GetIdentifier(payload.GetType());

        await _connection.AppendToStreamAsync(streamName, data, StreamPosition.Any);

    }
}
