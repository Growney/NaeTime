namespace EventDbLite.Streams;

public class StreamEvent(Guid id, string streamName, long streamOrdinal, long globalOrdinal, EventData data)
{
    public Guid Id { get; set; } = id;
    public string StreamName { get; set; } = streamName ?? throw new ArgumentNullException(nameof(streamName));
    public long StreamOrdinal { get; set; } = streamOrdinal;
    public long GlobalOrdinal { get; set; } = globalOrdinal;
    public EventData Data { get; set; } = data ?? throw new ArgumentNullException(nameof(data));
}
