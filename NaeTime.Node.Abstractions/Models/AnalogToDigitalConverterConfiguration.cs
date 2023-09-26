using NaeTime.Node.Abstractions.Enumeration;

namespace NaeTime.Node.Abstractions.Models;

public class AnalogToDigitalConverterConfiguration
{
    public byte Id { get; set; }
    public AnalogToDigitalConverterMode Mode { get; set; } = AnalogToDigitalConverterMode.HardwareSPI;
    public int BusId { get; set; } = 0;
    public int ChipSelectLine { get; set; } = 0;
    public int ClockFrequency { get; set; } = 3_000_000;
    public SpiMode SpiMode { get; set; } = SpiMode.Mode1;
    public int DataBitLength { get; set; } = 8;
}
