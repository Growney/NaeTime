namespace ImmersionRC.LapRF;
public struct ReceivedSignalStrengthIndicator(byte transponderId, float level, ulong? realTimeClockTime)
{
    public byte TransponderId { get; } = transponderId;
    public float Level { get; } = level;
    public ulong? RealTimeClockTime { get; } = realTimeClockTime;
}
