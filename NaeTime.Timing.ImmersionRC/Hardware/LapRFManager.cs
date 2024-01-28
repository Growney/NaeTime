using ImmersionRC.LapRF.Abstractions;
using Microsoft.Extensions.Hosting;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions.Notifications;
using NaeTime.Timing.Abstractions.Repositories;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : ISingletonSubscriber, IHostedService
{
    private readonly IHardwareRepository _hardwareRepository;
    private readonly ILapRFCommunicationFactory _communicationFactory;
    private readonly ILapRFProtocolFactory _protocolFactory;
    private readonly IDispatcher _dispatcher;

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(IHardwareRepository hardwareRepository, IDispatcher dispatcher, ILapRFCommunicationFactory communicationFactory, ILapRFProtocolFactory protocolFactory)
    {
        _dispatcher = dispatcher ?? throw new ArgumentNullException(nameof(dispatcher));
        _hardwareRepository = hardwareRepository;
        _communicationFactory = communicationFactory;
        _protocolFactory = protocolFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var devices = await _hardwareRepository.GetAllEthernetLapRF8ChannelAsync();

        foreach (var device in devices)
        {
            var connection = CreateEthernetCommunication(device.TimerId, device.Address, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }

    private LapRFConnection CreateEthernetCommunication(Guid timerId, IPAddress address, int port)
    {
        var communication = _communicationFactory.CreateEthernetCommunication(address, port);
        var protocol = _protocolFactory.Create(communication);
        return new LapRFConnection(timerId, _dispatcher, communication, protocol);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in _hardwareProcesses)
        {
            await device.Value.Stop();
        }
    }

    public async Task When(EthernetLapRF8ChannelTimerConfigured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.TimerId, out var connection))
        {
            await connection.Stop();
        }

        var newConnection = CreateEthernetCommunication(configured.TimerId, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.TimerId, newConnection,
            (id, existing) => newConnection);
    }
}
