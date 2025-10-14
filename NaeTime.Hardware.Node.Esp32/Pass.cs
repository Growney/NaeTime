namespace NaeTime.Hardware.Node.Esp32;
public struct Pass(byte lane, ulong time)
{
    public byte Lane { get; } = lane;
    public ulong Time { get; } = time;
}
