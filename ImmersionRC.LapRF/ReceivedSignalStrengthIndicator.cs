namespace ImmersionRC.LapRF;
public struct ReceivedSignalStrengthIndicator(byte laneId, float level, ulong? realTimeClockTime)
{
    public byte LaneId { get; } = laneId;
    public float Level { get; } = level;
    public ulong? RealTimeClockTime { get; } = realTimeClockTime;
}
