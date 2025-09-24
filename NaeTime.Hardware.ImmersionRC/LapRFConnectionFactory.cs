using ImmersionRC.LapRF.Abstractions;
using NaeTime.Hardware.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Net;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnectionFactory : ILapRFConnectionFactory
{
    private readonly ILapRFCommunicationFactory _communicationFactory;
    private readonly ILapRFProtocolFactory _protocolFactory;
    private readonly ISoftwareTimer _softwareTimer;

    public LapRFConnectionFactory(ILapRFCommunicationFactory communicationFactory, ILapRFProtocolFactory protocolFactory, ISoftwareTimer softwareTimer)
    {
        _communicationFactory = communicationFactory ?? throw new ArgumentNullException(nameof(communicationFactory));
        _protocolFactory = protocolFactory ?? throw new ArgumentNullException(nameof(protocolFactory));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
    }

    public LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, int port)
    {
        ILapRFCommunication communication = _communicationFactory.CreateEthernetCommunication(address, port);
        ILapRFProtocol protocol = _protocolFactory.Create(communication);
        return new LapRFConnection(timerId, _softwareTimer, communication, protocol);
    }
}
