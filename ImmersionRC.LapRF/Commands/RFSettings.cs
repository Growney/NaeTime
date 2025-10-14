namespace ImmersionRC.LapRF.Commands;
internal class RFSettings(byte pilotId, long realTimeClockTime, short statusFlag, bool isEnabled, short channel, short band, float threshold, short gain, short frequency) : CommandBase(pilotId, realTimeClockTime, statusFlag)
{
    public bool IsEnabled { get; } = isEnabled;
    public short Channel { get; } = channel;
    public short Band { get; } = band;
    public float Threshold { get; } = threshold;
    public short Gain { get; } = gain;
    public short Frequency { get; } = frequency;
}
