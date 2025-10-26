namespace ImmersionRC.LapRF;
public struct Pass(uint passingNumber, byte pilotId, byte laneId, uint timestamp, ulong realTimeClockTime)
{
    public uint PassingNumber { get; } = passingNumber;
    public byte PilotId { get; } = pilotId;
    public byte LaneId { get; } = laneId;
    public uint Timestamp { get; } = timestamp;
    public ulong RealTimeClockTime { get; } = realTimeClockTime;

}
