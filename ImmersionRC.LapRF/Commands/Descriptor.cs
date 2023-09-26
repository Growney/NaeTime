namespace ImmersionRC.LapRF.Commands;
internal class Descriptor : CommandBase
{
    public Descriptor(byte pilotId, long realTimeClockTime, short statusFlag, int puckVersion, byte protocolVersion) : base(pilotId, realTimeClockTime, statusFlag)
    {
        PuckVersion = puckVersion;
        ProtocolVersion = protocolVersion;
    }

    public int PuckVersion { get; }
    public byte ProtocolVersion { get; }
}
