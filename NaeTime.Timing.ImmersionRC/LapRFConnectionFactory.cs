using ImmersionRC.LapRF.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Net;

namespace NaeTime.Timing.ImmersionRC;
public class LapRFConnectionFactory : ILapRFConnectionFactory
{
    private readonly ILapRFCommunicationFactory _communicationFactory;
    private readonly ILapRFProtocolFactory _protocolFactory;
    private readonly IDispatcher _dispatcher;
    private readonly ISoftwareTimer _softwareTimer;

    public LapRFConnectionFactory(ILapRFCommunicationFactory communicationFactory, ILapRFProtocolFactory protocolFactory, IDispatcher dispatcher, ISoftwareTimer softwareTimer)
    {
        _communicationFactory = communicationFactory ?? throw new ArgumentNullException(nameof(communicationFactory));
        _protocolFactory = protocolFactory ?? throw new ArgumentNullException(nameof(protocolFactory));
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, int port)
    {
        var communication = _communicationFactory.CreateEthernetCommunication(address, port);
        var protocol = _protocolFactory.Create(communication);
        return new LapRFConnection(timerId, _softwareTimer, _dispatcher, communication, protocol);
    }
}
