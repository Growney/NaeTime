using Tensor.Mapping.Abstractions;

namespace NaeTime.Node.WebAPI.Map;

public class NodeConfigurationMap : IBidirectionalMapper<NodeConfiguration, NodeConfigurationDto>
{
    private readonly IMapper _mapper;

    public NodeConfigurationMap(IMapper mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public NodeConfiguration Map(NodeConfigurationDto source, NodeConfiguration? destination)
    {
        if (destination == null)
        {
            destination = new NodeConfiguration();
        }

        destination.NodeId = source.NodeId;
        destination.ServerAddress = source.ServerAddress;
        destination.AnalogToDigitalConverters = _mapper.Map<AnalogToDigitalConverterConfigurationDto, AnalogToDigitalConverterConfiguration>(source.AnalogToDigitalConverters).ToList();
        destination.MultiplexerConfigurations = _mapper.Map<MultiplexerConfigurationDto, MultiplexerConfiguration>(source.MultiplexerConfigurations).ToList();
        destination.MultiplexedRX5808Configurations = _mapper.Map<MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto, MultiplexedAnalogToDigitalConverterRx5808Configuration>(source.MultiplexedRX5808Configurations).ToList();
        destination.RX5808Configurations = _mapper.Map<AnalogToDigitalConverterRX5808ConfigurationDto, AnalogToDigitalConverterRX5808Configuration>(source.RX5808Configurations).ToList();

        return destination;
    }

    public NodeConfigurationDto Map(NodeConfiguration source, NodeConfigurationDto? destination)
    {
        if (destination == null)
        {
            destination = new NodeConfigurationDto();
        }

        destination.NodeId = source.NodeId;
        destination.ServerAddress = source.ServerAddress;
        destination.AnalogToDigitalConverters = _mapper.Map<AnalogToDigitalConverterConfiguration, AnalogToDigitalConverterConfigurationDto>(source.AnalogToDigitalConverters);
        destination.MultiplexerConfigurations = _mapper.Map<MultiplexerConfiguration, MultiplexerConfigurationDto>(source.MultiplexerConfigurations);
        destination.MultiplexedRX5808Configurations = _mapper.Map<MultiplexedAnalogToDigitalConverterRx5808Configuration, MultiplexedAnalogToDigitalConverterRx5808ConfigurationDto>(source.MultiplexedRX5808Configurations);
        destination.RX5808Configurations = _mapper.Map<AnalogToDigitalConverterRX5808Configuration, AnalogToDigitalConverterRX5808ConfigurationDto>(source.RX5808Configurations);

        return destination;
    }
}