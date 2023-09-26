namespace NaeTime.Node.WebAPI.Map;

public class AnalogToDigitalConverterRX5808ConfigurationMap : IBidirectionalMapper<AnalogToDigitalConverterRX5808Configuration, AnalogToDigitalConverterRX5808ConfigurationDto>
{
    public AnalogToDigitalConverterRX5808Configuration Map(AnalogToDigitalConverterRX5808ConfigurationDto source, AnalogToDigitalConverterRX5808Configuration? destination)
    {
        if (destination == null)
        {
            destination = new AnalogToDigitalConverterRX5808Configuration();
        }
        destination.Id = source.Id;
        destination.DataPin = source.DataPin;
        destination.SelectPin = source.SelectPin;
        destination.ClockPin = source.ClockPin;
        destination.ADCId = source.ADCId;
        destination.ADCChannel = source.ADCChannel;
        destination.Frequency = source.Frequency;
        destination.IsEnabled = source.IsEnabled;

        return destination;
    }

    public AnalogToDigitalConverterRX5808ConfigurationDto Map(AnalogToDigitalConverterRX5808Configuration source, AnalogToDigitalConverterRX5808ConfigurationDto? destination)
    {
        if (destination == null)
        {
            destination = new AnalogToDigitalConverterRX5808ConfigurationDto();
        }
        destination.Id = source.Id;
        destination.DataPin = source.DataPin;
        destination.SelectPin = source.SelectPin;
        destination.ClockPin = source.ClockPin;
        destination.ADCId = source.ADCId;
        destination.ADCChannel = source.ADCChannel;
        destination.Frequency = source.Frequency;
        destination.IsEnabled = source.IsEnabled;

        return destination;
    }
}
