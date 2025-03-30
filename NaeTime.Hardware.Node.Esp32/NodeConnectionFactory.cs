using Microsoft.Extensions.DependencyInjection;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnectionFactory : INodeConnectionFactory
{
    private readonly IServiceProvider _serviceProvider;
    public NodeConnectionFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public NodeConnection CreateSerialConnection(Guid timerId, string port)
    {
        INodeTimingProtocol timingProtocol = new NodeTimingProtocol();

        INodeCommunication communication = new NodeSerialCommunication(port);
        INodeConfigurationProtocol configurationProtocol = new NodeConfigurationProtocol(communication);
        INodeProtocol protocol = new NodeProtocol(communication, timingProtocol, configurationProtocol);
        return ActivatorUtilities.CreateInstance<NodeConnection>(_serviceProvider, timerId, protocol, communication);
    }
}
