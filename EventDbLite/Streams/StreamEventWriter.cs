using EventDbLite.Abstractions;

namespace EventDbLite.Streams;
public class StreamEventWriter(IEventSerializer eventSerializer, IEventStoreLite connection) : IStreamEventWriter
{
    private readonly IEventSerializer _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    private readonly IEventStoreLite _connection = connection ?? throw new ArgumentNullException(nameof(connection));

    public async Task AppendToStream(string streamName, IEnumerable<object> payload)
    {
        if (string.IsNullOrWhiteSpace(streamName))
        {
            throw new ArgumentException("Stream name must not be null or whitespace.", nameof(streamName));
        }
        if (payload == null)
        {
            throw new ArgumentNullException(nameof(payload));
        }

        List<EventData> eventData = new();

        foreach(var item in payload)
        {
            EventMetadata metaData = _eventSerializer.CreateMetadata(item);
            byte[] metadataPayload = _eventSerializer.SerializeMetadata(metaData);
            byte[] eventPayload = _eventSerializer.SerializeEvent(item);
            EventData data = new(eventPayload, metadataPayload, metaData.Identifier);
            eventData.Add(data);
        }

        await _connection.AppendToStreamAsync(streamName, eventData, StreamPosition.Any);
    }
}
