namespace ImmersionRC.LapRF.Commands;
internal class RFSettings : CommandBase
{
    public RFSettings(byte pilotId, long realTimeClockTime, short statusFlag, bool isEnabled, short channel, short band, float threshold, short gain, short frequency)
        : base(pilotId, realTimeClockTime, statusFlag)
    {
        IsEnabled = isEnabled;
        Channel = channel;
        Band = band;
        Threshold = threshold;
        Gain = gain;
        Frequency = frequency;
    }

    public bool IsEnabled { get; }
    public short Channel { get; }
    public short Band { get; }
    public float Threshold { get; }
    public short Gain { get; }
    public short Frequency { get; }
}
