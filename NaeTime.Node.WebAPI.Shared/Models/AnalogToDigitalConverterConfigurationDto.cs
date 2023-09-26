using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.WebAPI.Shared.Enumeration;

namespace NaeTime.Node.WebAPI.Shared.Models;

public class AnalogToDigitalConverterConfigurationDto
{
    public byte Id { get; set; }
    public AnalogToDigitalConverterModeDto Mode { get; set; } = AnalogToDigitalConverterModeDto.HardwareSPI;
    public int BusId { get; set; } = 0;
    public int ChipSelectLine { get; set; } = 0;
    public int ClockFrequency { get; set; } = 3_000_000;
    public SpiModeDto SpiMode { get; set; } = SpiModeDto.Mode1;
    public int DataBitLength { get; set; } = 8;
}
