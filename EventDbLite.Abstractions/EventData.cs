namespace EventDbLite.Abstractions;

public class EventData
{
    public byte[] Metadata { get; }
    public byte[] Payload { get; }
    public string Identifier { get; }

    public EventData(byte[] payload, byte[] metadata, string identifier)
    {
        Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
        Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }
}
