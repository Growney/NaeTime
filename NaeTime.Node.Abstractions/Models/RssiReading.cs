namespace NaeTime.Node.Abstractions.Models;

public struct RssiReading
{
    public RssiReading(long tick, int value, int frequency, byte deviceId)
    {
        Tick = tick;
        Value = value;
        Frequency = frequency;
        DeviceId = deviceId;
    }

    public long Tick { get; }
    public int Value { get; }
    public int Frequency { get; }
    public byte DeviceId { get; }

}
