namespace ImmersionRC.LapRF.Commands;
internal class CommandBase
{
    public CommandBase(byte pilotId, long realTimeClockTime, short statusFlag)
    {
        PilotId = pilotId;
        RealTimeClockTime = realTimeClockTime;
        StatusFlag = statusFlag;
    }

    public byte PilotId { get; }
    public long RealTimeClockTime { get; }
    public short StatusFlag { get; }
}
