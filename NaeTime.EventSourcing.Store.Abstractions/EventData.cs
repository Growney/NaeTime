namespace NaeTime.EventSourcing.Store.Abstractions;
public class EventData
{
    public ReadOnlyMemory<byte> Data { get; }
    public ReadOnlyMemory<byte> Metadata { get; }

    public EventData(ReadOnlyMemory<byte> data, ReadOnlyMemory<byte> metadata)
    {
        Data = data;
        Metadata = metadata;
    }
}
