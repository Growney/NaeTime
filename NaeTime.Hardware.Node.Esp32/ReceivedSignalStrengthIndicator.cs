namespace NaeTime.Hardware.Node.Esp32;
public struct ReceivedSignalStrengthIndicator
{
    public ReceivedSignalStrengthIndicator(byte lane, ushort level, ulong? realTimeClockTime)
    {
        Lane = lane;
        Level = level;
        RealTimeClockTime = realTimeClockTime;
    }

    public byte Lane { get; }
    public ushort Level { get; }
    public ulong? RealTimeClockTime { get; }
}
