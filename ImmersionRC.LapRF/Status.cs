namespace ImmersionRC.LapRF;
public struct Status
{
    public Status(ushort? inputVoltage, byte? gateState, ushort? statusFlags, uint? statusCount)
    {
        InputVoltage = inputVoltage;
        GateState = gateState;
        StatusFlags = statusFlags;
        StatusCount = statusCount;
    }

    public ushort? InputVoltage { get; }
    public byte? GateState { get; }
    public ushort? StatusFlags { get; }
    public uint? StatusCount { get; }
}
