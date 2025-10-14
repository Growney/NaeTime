namespace NaeTime.Hardware.Node.Esp32;
public struct ReceivedSignalStrengthIndicator(byte lane, ushort level, ulong? realTimeClockTime)
{
    public byte Lane { get; } = lane;
    public ushort Level { get; } = level;
    public ulong? RealTimeClockTime { get; } = realTimeClockTime;
}
