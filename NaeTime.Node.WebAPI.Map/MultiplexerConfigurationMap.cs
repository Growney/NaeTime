namespace NaeTime.Node.WebAPI.Map;

public class MultiplexerConfigurationMap : IBidirectionalMapper<MultiplexerConfiguration, MultiplexerConfigurationDto>
{
    public MultiplexerConfiguration Map(MultiplexerConfigurationDto source, MultiplexerConfiguration? destination)
    {
        if (destination == null)
        {
            destination = new MultiplexerConfiguration();
        }
        destination.Id = source.Id;
        destination.AAddressPin = source.AAddressPin;
        destination.BAddressPin = source.BAddressPin;
        destination.CAddressPin = source.CAddressPin;
        destination.DataPin = source.DataPin;
        destination.SelectPin = source.SelectPin;
        destination.ClockPin = source.ClockPin;

        return destination;
    }

    public MultiplexerConfigurationDto Map(MultiplexerConfiguration source, MultiplexerConfigurationDto? destination)
    {
        if (destination == null)
        {
            destination = new MultiplexerConfigurationDto();
        }
        destination.Id = source.Id;
        destination.AAddressPin = source.AAddressPin;
        destination.BAddressPin = source.BAddressPin;
        destination.CAddressPin = source.CAddressPin;
        destination.DataPin = source.DataPin;
        destination.SelectPin = source.SelectPin;
        destination.ClockPin = source.ClockPin;

        return destination;
    }
}
