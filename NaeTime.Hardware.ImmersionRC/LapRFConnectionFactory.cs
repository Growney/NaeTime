using ImmersionRC.LapRF.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Net;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnectionFactory : ILapRFConnectionFactory
{
    private readonly ILapRFCommunicationFactory _communicationFactory;
    private readonly ILapRFProtocolFactory _protocolFactory;
    private readonly IServiceProvider _serviceProvider;

    public LapRFConnectionFactory(ILapRFCommunicationFactory communicationFactory, ILapRFProtocolFactory protocolFactory, IServiceProvider provider)
    {
        _communicationFactory = communicationFactory ?? throw new ArgumentNullException(nameof(communicationFactory));
        _protocolFactory = protocolFactory ?? throw new ArgumentNullException(nameof(protocolFactory));
        _serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
    }

    public LapRFConnection CreateEthernetConnection(Guid timerId, IPAddress address, int port)
    {
        ILapRFCommunication communication = _communicationFactory.CreateEthernetCommunication(address, port);
        ILapRFProtocol protocol = _protocolFactory.Create(communication);
        return ActivatorUtilities.CreateInstance<LapRFConnection>(_serviceProvider, timerId, protocol, communication);
    }
}
