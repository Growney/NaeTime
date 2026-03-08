using EventDbLite.Streams;

namespace EventDbLite.Abstractions;

public interface IEventStoreLite
{
    IAsyncEnumerable<StreamEvent> ReadStreamEvents(string streamName, StreamDirection direction, StreamPosition fromPosition);
    IAsyncEnumerable<StreamEvent> ReadEvents(StreamDirection direction, StreamPosition fromPosition);

    Task AppendToStreamAsync(string streamName, IEnumerable<EventData> data, StreamPosition expectedState);

    IStreamSubscription SubscribeToStream(string streamName, StreamPosition from);
    IStreamSubscription SubscribeToAllStreams(StreamPosition from);
}
