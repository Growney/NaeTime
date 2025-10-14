
namespace EventDbLite.Streams;

public class EventData(byte[] payload, byte[] metadata)
{
    public byte[] Metadata { get; } = metadata ?? throw new ArgumentNullException(nameof(metadata));
    public byte[] Payload { get; } = payload ?? throw new ArgumentNullException(nameof(payload));
}
