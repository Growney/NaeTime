namespace ImmersionRC.LapRF;
public struct ReceivedSignalStrengthIndicator
{
    public ReceivedSignalStrengthIndicator(byte transponderId, float level, ulong? realTimeClockTime)
    {
        TransponderId = transponderId;
        Level = level;
        RealTimeClockTime = realTimeClockTime;
    }

    public byte TransponderId { get; }
    public float Level { get; }
    public ulong? RealTimeClockTime { get; }
}
