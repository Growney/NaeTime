namespace NaeTime.Hardware.Node.Esp32;
public struct Pass
{
    public byte Lane { get; }
    public ulong Time { get; }

    public Pass(byte lane, ulong time)
    {
        Lane = lane;
        Time = time;
    }

}
