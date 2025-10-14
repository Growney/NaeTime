namespace ImmersionRC.LapRF;
public struct Status(ushort? inputVoltage, byte? gateState, ushort? statusFlags, uint? statusCount)
{
    public ushort? InputVoltage { get; } = inputVoltage;
    public byte? GateState { get; } = gateState;
    public ushort? StatusFlags { get; } = statusFlags;
    public uint? StatusCount { get; } = statusCount;
}
