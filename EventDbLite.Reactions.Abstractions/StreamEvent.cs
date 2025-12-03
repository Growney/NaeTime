namespace EventDbLite.Reactions.Abstractions;

public class StreamEvent
{
    public Guid Id { get; set; }
    public string StreamName { get; set; }
    public long StreamOrdinal { get; set; }
    public long GlobalOrdinal { get; set; }
    public EventData Data { get; set; }

    public StreamEvent(Guid id, string streamName, long streamOrdinal, long globalOrdinal, EventData data)
    {
        Id = id;
        StreamName = streamName ?? throw new ArgumentNullException(nameof(streamName));
        StreamOrdinal = streamOrdinal;
        GlobalOrdinal = globalOrdinal;
        Data = data ?? throw new ArgumentNullException(nameof(data));
    }
}
