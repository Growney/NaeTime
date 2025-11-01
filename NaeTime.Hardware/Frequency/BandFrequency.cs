namespace NaeTime.Hardware.Frequency;
public struct BandFrequency(byte bandId,string name, int frequency)
{
    public byte BandId { get; } = bandId;
    public string Name { get; } = name ?? throw new ArgumentNullException(nameof(name));
    public int FrequencyInMhz { get; } = frequency;
}
