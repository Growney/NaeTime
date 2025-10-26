namespace ImmersionRC.LapRF;
public struct RFSetup(byte laneId, bool isEnabled, ushort? channel, ushort? band, ushort? attenuation, ushort? frequency, float? threshold)
{
    public byte LaneId { get; } = laneId;
    public bool IsEnabled { get; } = isEnabled;
    public ushort? Channel { get; } = channel;
    public ushort? Band { get; } = band;
    public ushort? Attenuation { get; } = attenuation;
    public ushort? Frequency { get; } = frequency;
    public float? Threshold { get; } = threshold;
}
