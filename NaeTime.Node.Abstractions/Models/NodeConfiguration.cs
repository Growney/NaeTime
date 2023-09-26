namespace NaeTime.Node.Abstractions.Models;

public class NodeConfiguration
{
    public Guid NodeId { get; set; }
    public string? ServerAddress { get; set; }
    public List<AnalogToDigitalConverterConfiguration> AnalogToDigitalConverters { get; set; } = new List<AnalogToDigitalConverterConfiguration>();
    public List<MultiplexerConfiguration> MultiplexerConfigurations { get; set; } = new List<MultiplexerConfiguration>();
    public List<MultiplexedAnalogToDigitalConverterRx5808Configuration> MultiplexedRX5808Configurations { get; set; } = new List<MultiplexedAnalogToDigitalConverterRx5808Configuration>();
    public List<AnalogToDigitalConverterRX5808Configuration> RX5808Configurations { get; set; } = new List<AnalogToDigitalConverterRX5808Configuration>();
}
