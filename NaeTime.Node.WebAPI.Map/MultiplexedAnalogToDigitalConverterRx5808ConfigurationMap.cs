namespace NaeTime.Node.WebAPI.Map;

public class MultiplexedAnalogToDigitalConverterRx5808ConfigurationMap : IBidirectionalMapper<MultiplexedAnalogToDigitalConverterRx5808Configuration, MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto>
{
    public MultiplexedAnalogToDigitalConverterRx5808Configuration Map(MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto source, MultiplexedAnalogToDigitalConverterRx5808Configuration? destination)
    {
        if (destination == null)
        {
            destination = new MultiplexedAnalogToDigitalConverterRx5808Configuration();
        }
        destination.Id = source.Id;
        destination.MultiplexerId = source.MultiplexerId;
        destination.MultiplexerChannel = source.MultiplexerChannel;
        destination.ADCId = source.ADCId;
        destination.ADCChannel = source.ADCChannel;
        destination.Frequency = source.Frequency;
        destination.IsEnabled = source.IsEnabled;

        return destination;
    }

    public MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto Map(MultiplexedAnalogToDigitalConverterRx5808Configuration source, MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto? destination)
    {
        if (destination == null)
        {
            destination = new MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto();
        }
        destination.Id = source.Id;
        destination.MultiplexerId = source.MultiplexerId;
        destination.MultiplexerChannel = source.MultiplexerChannel;
        destination.ADCId = source.ADCId;
        destination.ADCChannel = source.ADCChannel;
        destination.Frequency = source.Frequency;
        destination.IsEnabled = source.IsEnabled;

        return destination;
    }
}
