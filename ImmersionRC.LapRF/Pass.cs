namespace ImmersionRC.LapRF;
public struct Pass(uint passingNumber, byte pilotId, uint transponderId, uint timestamp, ulong realTimeClockTime)
{
    public uint PassingNumber { get; } = passingNumber;
    public byte PilotId { get; } = pilotId;
    public uint TransponderId { get; } = transponderId;
    public uint Timestamp { get; } = timestamp;
    public ulong RealTimeClockTime { get; } = realTimeClockTime;

}
