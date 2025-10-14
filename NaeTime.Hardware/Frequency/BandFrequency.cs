namespace NaeTime.Hardware.Frequency;
public struct BandFrequency(string name, int frequency)
{
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public int FrequencyInMhz { get; } = frequency;
}
