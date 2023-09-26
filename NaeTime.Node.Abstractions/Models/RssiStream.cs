namespace NaeTime.Node.Abstractions.Models;

public class RssiStream
{
    public RssiStream(Guid id, int frequency, byte deviceId, bool isEnabled)
    {
        Id = id;
        Frequency = frequency;
        DeviceId = deviceId;
        IsEnabled = isEnabled;
    }

    public Guid Id { get; }
    public int Frequency { get; }
    public byte DeviceId { get; }
    public bool IsEnabled { get; }
}
