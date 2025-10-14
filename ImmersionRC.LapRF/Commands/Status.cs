namespace ImmersionRC.LapRF.Commands;
internal class Status(byte pilotId, long realTimeClockTime, short statusFlag, short inputVoltage, float receivedSignalStrengthIndicator, byte gateState, int count) : CommandBase(pilotId, realTimeClockTime, statusFlag)
{
    public short InputVoltage { get; } = inputVoltage;
    public float ReceivedSignalStrengthIndicator { get; } = receivedSignalStrengthIndicator;
    public byte GateState { get; } = gateState;
    public int Count { get; } = count;
}
