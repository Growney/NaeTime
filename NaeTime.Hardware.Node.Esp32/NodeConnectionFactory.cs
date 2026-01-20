using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Net;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnectionFactory : INodeConnectionFactory
{
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IStreamEventWriter _writer;
    private readonly INaeTimeNodeCommandHandler _commandHandler;
    private readonly IRssiChannel _rssiChannel;

    public NodeConnectionFactory(ISoftwareTimer softwareTimer, IStreamEventWriter writer, INaeTimeNodeCommandHandler commandHandler,IRssiChannel rssiChannel)
    {
        _writer = writer;
        _commandHandler = commandHandler;
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _rssiChannel = rssiChannel;
    }

    public NodeConnection CreateSerialConnection(Guid timerId, string port)
    {
        INodeTimingProtocol timingProtocol = new NodeTimingProtocol();

        INodeCommunication communication = new NodeSerialCommunication(port);
        INodeConfigurationProtocol configurationProtocol = new NodeConfigurationProtocol(communication);
        INodeProtocol protocol = new NodeProtocol(communication, timingProtocol, configurationProtocol);
        return new NodeConnection(timerId, _softwareTimer, communication, protocol, _writer, _commandHandler, _rssiChannel);
    }

    public NodeConnection CreateTcpConnection(Guid timerId, IPAddress ipAddress, ushort port)
    {
        INodeTimingProtocol timingProtocol = new NodeTimingProtocol();
        INodeCommunication communication = new NodeTCPCommunication(ipAddress, port);
        INodeConfigurationProtocol configurationProtocol = new NodeConfigurationProtocol(communication);
        INodeProtocol protocol = new NodeProtocol(communication, timingProtocol, configurationProtocol);
        return new NodeConnection(timerId, _softwareTimer, communication, protocol, _writer, _commandHandler, _rssiChannel);
    }
}
