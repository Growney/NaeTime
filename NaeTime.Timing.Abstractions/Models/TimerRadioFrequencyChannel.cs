namespace NaeTime.Timing.Abstractions.Models;
public class TimerRadioFrequencyChannel
{
    public TimerRadioFrequencyChannel(byte channelId, int frequencyInHertz, bool isEnabled)
    {
        ChannelId = channelId;
        FrequencyInHertz = frequencyInHertz;
        IsEnabled = isEnabled;
    }

    public byte ChannelId { get; }
    public int FrequencyInHertz { get; }
    public bool IsEnabled { get; }
}
