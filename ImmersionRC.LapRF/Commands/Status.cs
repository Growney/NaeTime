namespace ImmersionRC.LapRF.Commands;
internal class Status : CommandBase
{
    public Status(byte pilotId, long realTimeClockTime, short statusFlag, short inputVoltage, float receivedSignalStrengthIndicator, byte gateState, int count)
        : base(pilotId, realTimeClockTime, statusFlag)
    {
        InputVoltage = inputVoltage;
        ReceivedSignalStrengthIndicator = receivedSignalStrengthIndicator;
        GateState = gateState;
        Count = count;
    }

    public short InputVoltage { get; }
    public float ReceivedSignalStrengthIndicator { get; }
    public byte GateState { get; }
    public int Count { get; }
}
