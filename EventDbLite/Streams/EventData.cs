
namespace EventDbLite.Streams;

public class EventData
{
    public EventData(byte[] payload, byte[] metadata)
    {
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    public byte[] Metadata { get; }
    public byte[] Payload { get; }
}
