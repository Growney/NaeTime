namespace EventDbLite.Abstractions;

public class StreamEvent
{
    public Guid Id { get; }
    public string StreamName { get; }
    public long StreamOrdinal { get; }
    public long GlobalOrdinal { get; }
    public EventData Data { get; }

    public StreamEvent(Guid id, string streamName, long streamOrdinal, long globalOrdinal, EventData data)
    {
        Id = id;
        StreamName = streamName ?? throw new ArgumentNullException(nameof(streamName));
        StreamOrdinal = streamOrdinal;
        GlobalOrdinal = globalOrdinal;
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}
