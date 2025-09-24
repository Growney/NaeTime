using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnectionFactory : INodeConnectionFactory
{
    private readonly ISoftwareTimer _softwareTimer;

    public NodeConnectionFactory(ISoftwareTimer softwareTimer)
    {
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public NodeConnection CreateSerialConnection(Guid timerId, string port)
    {
        INodeTimingProtocol timingProtocol = new NodeTimingProtocol();

        INodeCommunication communication = new NodeSerialCommunication(port);
        INodeConfigurationProtocol configurationProtocol = new NodeConfigurationProtocol(communication);
        INodeProtocol protocol = new NodeProtocol(communication, timingProtocol, configurationProtocol);
        return new NodeConnection(timerId, _softwareTimer, communication, protocol);
    }
}
