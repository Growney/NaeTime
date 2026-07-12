using EventDbLite.Abstractions;
using ImmersionRC.LapRF.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Hardware.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Net;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnectionFactory : ILapRFConnectionFactory
{
    private readonly ILapRFCommunicationFactory _communicationFactory;
    private readonly ILapRFProtocolFactory _protocolFactory;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IAggregateRepository _aggregateRepository;
    private readonly IImmersionRCLapRFCommandHandler _commandHandler;
    private readonly IRssiChannel _rssiChannel;

    public LapRFConnectionFactory(ILapRFCommunicationFactory communicationFactory, ILapRFProtocolFactory protocolFactory, ISoftwareTimer softwareTimer, IAggregateRepository aggregateRepository, IImmersionRCLapRFCommandHandler commandHandler, IRssiChannel rssiChannel)
    {
        _aggregateRepository = aggregateRepository;
        _commandHandler = commandHandler;
        _communicationFactory = communicationFactory ?? throw new ArgumentNullException(nameof(communicationFactory));
        _protocolFactory = protocolFactory ?? throw new ArgumentNullException(nameof(protocolFactory));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _rssiChannel = rssiChannel;
    }

    public LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, ushort port)
    {
        ILapRFCommunication communication = _communicationFactory.CreateEthernetCommunication(address, port);
        ILapRFProtocol protocol = _protocolFactory.Create(communication);
        return new LapRFConnection(timerId, _softwareTimer, communication, protocol, _aggregateRepository, _commandHandler, _rssiChannel);
    }
}
