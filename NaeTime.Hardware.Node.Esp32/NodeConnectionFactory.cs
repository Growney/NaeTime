using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnectionFactory : INodeConnectionFactory
{
    private readonly IEventClient _eventClient;
    private readonly ISoftwareTimer _softwareTimer;

    public NodeConnectionFactory(IEventClient eventClient, ISoftwareTimer softwareTimer)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public NodeConnection CreateSerialConnection(Guid timerId, string port)
    {
        INodeCommunication communication = new NodeSerialCommunication(port);
        INodeProtocol protocol = new NodeProtocol(communication);
        return new NodeConnection(timerId, _softwareTimer, _eventClient, communication, protocol);
    }
}
