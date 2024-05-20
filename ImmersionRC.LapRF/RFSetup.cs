﻿namespace ImmersionRC.LapRF;
public struct RFSetup
{
    public RFSetup(byte transponderId, bool isEnabled, ushort? channel, ushort? band, ushort? attenuation, ushort? frequency, float? threshold)
    {
        TransponderId = transponderId;
        IsEnabled = isEnabled;
        Channel = channel;
        Band = band;
        Attenuation = attenuation;
        Frequency = frequency;
        Threshold = threshold;
    }

    public byte TransponderId { get; }
    public bool IsEnabled { get; }
    public ushort? Channel { get; }
    public ushort? Band { get; }
    public ushort? Attenuation { get; }
    public ushort? Frequency { get; }
    public float? Threshold { get; }
}
