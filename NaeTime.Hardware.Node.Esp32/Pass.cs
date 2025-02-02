namespace NaeTime.Hardware.Node.Esp32;
public struct Pass
{
    public byte Lane { get; }
    public ulong PassStart { get; }
    public ulong PassEnd { get; }
    public ulong Time { get; }

    public Pass(byte lane, ulong passStart, ulong passEnd, ulong time)
    {
        Lane = lane;
        PassStart = passStart;
        PassEnd = passEnd;
        Time = time;
    }

}
