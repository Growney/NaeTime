namespace NaeTime.Node.WebAPI.Shared.Models;

public class NodeConfigurationDto
{
    public Guid NodeId { get; set; }
    public string? ServerAddress { get; set; }
    public IEnumerable<AnalogToDigitalConverterConfigurationDto> AnalogToDigitalConverters { get; set; } = Enumerable.Empty<AnalogToDigitalConverterConfigurationDto>();
    public IEnumerable<MultiplexerConfigurationDto> MultiplexerConfigurations { get; set; } = Enumerable.Empty<MultiplexerConfigurationDto>();
    public IEnumerable<MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto> MultiplexedRX5808Configurations { get; set; } = Enumerable.Empty<MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto>();
    public IEnumerable<AnalogToDigitalConverterRX5808ConfigurationDto> RX5808Configurations { get; set; } = Enumerable.Empty<AnalogToDigitalConverterRX5808ConfigurationDto>();
}
