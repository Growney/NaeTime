namespace ImmersionRC.LapRF;
public struct Pass
{
    public Pass(uint passingNumber, byte pilotId, uint transponderId, uint timestamp, ulong realTimeClockTime)
    {
        PassingNumber = passingNumber;
        PilotId = pilotId;
        TransponderId = transponderId;
        Timestamp = timestamp;
        RealTimeClockTime = realTimeClockTime;
    }

    public uint PassingNumber { get; }
    public byte PilotId { get; }                    // pilot ID, from 1 to max pilots
    public uint TransponderId { get; }
    public uint Timestamp { get; }
    public ulong RealTimeClockTime { get; }                      // in microseconds

}
