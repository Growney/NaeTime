
namespace NaeTime.Timing.Frequency;
public struct BandFrequency
{
    public BandFrequency(string name, int frequency)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Frequency = frequency;
    }

    public string Name { get; }
    public int Frequency { get; }
}
