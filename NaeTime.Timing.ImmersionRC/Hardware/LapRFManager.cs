using Microsoft.Extensions.Hosting;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions.Notifications;
using NaeTime.Timing.Abstractions.Repositories;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : IHostedService
{
    private readonly IHardwareRepository _hardwareRepository;
    private readonly IPublisher _publisher;
    private readonly ILapRFConnectionFactory _connectionFactory;
    private readonly IDispatcher _dispatcher;

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(IHardwareRepository hardwareRepository, IPublisher publisher, ILapRFConnectionFactory connectionFactory, IDispatcher dispatcher)
    {
        _hardwareRepository = hardwareRepository ?? throw new ArgumentNullException(nameof(hardwareRepository));
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _dispatcher = dispatcher;

        _publisher.Subscribe<EthernetLapRF8ChannelTimerConnectionConfigured>(this, When);
        _publisher.Subscribe<TimerRadioFrequenciesRequested>(this, When);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var devices = await _hardwareRepository.GetAllEthernetLapRF8ChannelAsync();

        foreach (var device in devices)
        {
            var connection = _connectionFactory.CreateEthernetConnection(device.TimerId, device.Address, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in _hardwareProcesses)
        {
            await device.Value.Stop();
        }

        _publisher.Unsubscribe(this);
    }

    private async Task When(EthernetLapRF8ChannelTimerConnectionConfigured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.TimerId, out var connection))
        {
            await connection.Stop();
        }

        var newConnection = _connectionFactory.CreateEthernetConnection(configured.TimerId, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.TimerId, newConnection,
            (id, existing) => newConnection);
    }
    private async Task When(TimerRadioFrequenciesRequested requested)
    {
        if (!_hardwareProcesses.TryGetValue(requested.TimerId, out var connection))
        {
            return;
        }

        if (!connection.IsConnected)
        {
            return;
        }

        var frequencies = await connection.GetRadioFrequencyChannelsAsync();

        await _dispatcher.Dispatch(new TimerRadioFrequenciesAquired(requested.TimerId, frequencies)).ConfigureAwait(false);
    }
}
