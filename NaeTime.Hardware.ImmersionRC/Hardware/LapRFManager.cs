using EventDbLite.Abstractions;
using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : BackgroundService
{
    private readonly ILapRFConnectionFactory _connectionFactory;
    private readonly IHardwareQueryHandler _queryHandler;
    private readonly ILapRFConnectionProvider _connectionProvider;
    private readonly IReactionProviderFactory _reactionProviderFactory;

    private readonly ConcurrentDictionary<Guid, ILapRFConnection> _hardwareProcesses = new();

    public LapRFManager(ILapRFConnectionFactory connectionFactory, IHardwareQueryHandler queryHandler, ILapRFConnectionProvider connectionProvider, IReactionProviderFactory reactionProviderFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _reactionProviderFactory = reactionProviderFactory ?? throw new ArgumentException("Connection factory must implement IServiceProvider to allow access to reaction provider.", nameof(connectionFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IEnumerable<Query.Abstractions.Models.ImmersionRCLapRF> devices = await _queryHandler.GetAllImmersionRCLapRFs();

        foreach (Query.Abstractions.Models.ImmersionRCLapRF device in devices)
        {
            ILapRFConnection connection = device switch
            {
                NaeTime.Query.Abstractions.Models.Ethernet8ChannelImmersionRCLapRF ethernetDevice => _connectionFactory.CreateEthernetConnection(ethernetDevice.Id, ethernetDevice.IPAddress, ethernetDevice.Port),
                _ => throw new NotSupportedException($"LapRF device type {device.GetType().FullName} is not supported.")
            };

            _connectionProvider.SetLapRFConnection(device.Id, connection);

            _hardwareProcesses[device.Id] = connection;
        }

        await Task.WhenAll(
            _reactionProviderFactory.On<Events.ImmersionRCLapRFNetworkConfigurationChanged>(HandleNetworkConfigurationChange, stoppingToken),
            _reactionProviderFactory.On<Events.ImmersionRCLapRFNetworkDeviceRegistered>(HandleNetworkDeviceRegistered, stoppingToken));
    }

    private async Task HandleNetworkConfigurationChange(Events.ImmersionRCLapRFNetworkConfigurationChanged e)
    {
        if (_hardwareProcesses.TryGetValue(e.TimerId, out ILapRFConnection? existingConnection))
        {
            await existingConnection.Stop();
            _hardwareProcesses.TryRemove(e.TimerId, out _);
        }
        if (IPAddress.TryParse(e.IPAddress, out IPAddress? ipAddress))
        {
            ILapRFConnection connection = _connectionFactory.CreateEthernetConnection(e.TimerId, ipAddress, e.Port);
            _connectionProvider.SetLapRFConnection(e.TimerId, connection);
            _hardwareProcesses[e.TimerId] = connection;
        }
    }

    private async Task HandleNetworkDeviceRegistered(Events.ImmersionRCLapRFNetworkDeviceRegistered e)
    {
        if (_hardwareProcesses.TryGetValue(e.TimerId, out ILapRFConnection? existingConnection))
        {
            await existingConnection.Stop();
            _hardwareProcesses.TryRemove(e.TimerId, out _);
        }
        if (IPAddress.TryParse(e.IPAddress, out IPAddress? ipAddress))
        {
            ILapRFConnection connection = _connectionFactory.CreateEthernetConnection(e.TimerId, ipAddress, e.Port);
            _connectionProvider.SetLapRFConnection(e.TimerId, connection);
            _hardwareProcesses[e.TimerId] = connection;
        }
    }
}
