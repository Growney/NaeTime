namespace NaeTime.Node.WebAPI.Map;

public class AnalogToDigitalConverterConfigurationMap : IBidirectionalMapper<AnalogToDigitalConverterConfiguration, AnalogToDigitalConverterConfigurationDto>
{

    public AnalogToDigitalConverterConfiguration Map(AnalogToDigitalConverterConfigurationDto source, AnalogToDigitalConverterConfiguration? destination)
    {
        if (destination == null)
        {
            destination = new AnalogToDigitalConverterConfiguration();
        }
        destination.Id = source.Id;
        destination.Mode = source.Mode switch
        {
            AnalogToDigitalConverterModeDto.HardwareSPI => AnalogToDigitalConverterMode.HardwareSPI,
            _ => throw new NotImplementedException()
        };
        destination.BusId = source.BusId;
        destination.ChipSelectLine = source.ChipSelectLine;
        destination.ClockFrequency = source.ClockFrequency;
        destination.SpiMode = source.SpiMode switch
        {
            SpiModeDto.Mode0 => SpiMode.Mode0,
            SpiModeDto.Mode1 => SpiMode.Mode1,
            SpiModeDto.Mode2 => SpiMode.Mode2,
            SpiModeDto.Mode3 => SpiMode.Mode3,
            _ => throw new NotImplementedException()
        };
        destination.DataBitLength = source.DataBitLength;

        return destination;

    }

    public AnalogToDigitalConverterConfigurationDto Map(AnalogToDigitalConverterConfiguration source, AnalogToDigitalConverterConfigurationDto? destination)
    {
        if (destination == null)
        {
            destination = new AnalogToDigitalConverterConfigurationDto();
        }
        destination.Id = source.Id;
        destination.Mode = source.Mode switch
        {
            AnalogToDigitalConverterMode.HardwareSPI => AnalogToDigitalConverterModeDto.HardwareSPI,
            _ => throw new NotImplementedException()
        };
        destination.BusId = source.BusId;
        destination.ChipSelectLine = source.ChipSelectLine;
        destination.ClockFrequency = source.ClockFrequency;
        destination.SpiMode = source.SpiMode switch
        {
            SpiMode.Mode0 => SpiModeDto.Mode0,
            SpiMode.Mode1 => SpiModeDto.Mode1,
            SpiMode.Mode2 => SpiModeDto.Mode2,
            SpiMode.Mode3 => SpiModeDto.Mode3,
            _ => throw new NotImplementedException()
        };
        destination.DataBitLength = source.DataBitLength;

        return destination;
    }
}
