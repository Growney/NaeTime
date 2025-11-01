using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IEventStreamConnection
{
    IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, StreamPosition fromPosition);
    IAsyncEnumerable<StreamEvent> ReadAllStreamEvents(StreamDirection direction, StreamPosition fromPosition);

    Task<IEnumerable<StreamEvent>> AppendToStreamAsync(string streamName, IEnumerable<EventData> data, StreamPosition expectedState);
    Task<StreamEvent> AppendToStreamAsync(string streamName, EventData data, StreamPosition expectedState);
}
