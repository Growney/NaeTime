namespace NaeTime.Messages.Frequency;
public struct BandFrequency
{
    public BandFrequency(string name, int frequency)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        FrequencyInMhz = frequency;
    }

    public string Name { get; }
    public int FrequencyInMhz { get; }
}
