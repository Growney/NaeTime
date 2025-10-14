namespace ImmersionRC.LapRF.Commands;
internal class CommandBase(byte pilotId, long realTimeClockTime, short statusFlag)
{
    public byte PilotId { get; } = pilotId;
    public long RealTimeClockTime { get; } = realTimeClockTime;
    public short StatusFlag { get; } = statusFlag;
}
