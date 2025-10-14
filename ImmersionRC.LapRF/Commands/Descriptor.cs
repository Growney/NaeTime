namespace ImmersionRC.LapRF.Commands;
internal class Descriptor(byte pilotId, long realTimeClockTime, short statusFlag, int puckVersion, byte protocolVersion) : CommandBase(pilotId, realTimeClockTime, statusFlag)
{
    public int PuckVersion { get; } = puckVersion;
    public byte ProtocolVersion { get; } = protocolVersion;
}
