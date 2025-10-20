using EventDbLite.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : IHostedService
{
    private readonly ILapRFConnectionFactory _connectionFactory;
    private readonly IHardwareQueryHandler _queryHandler;
    private readonly ILapRFConnectionProvider _connectionProvider;
    private readonly IServiceProvider _serviceProvider;

    private readonly ConcurrentDictionary<Guid, ILapRFConnection> _hardwareProcesses = new();

    public LapRFManager(ILapRFConnectionFactory connectionFactory, IHardwareQueryHandler queryHandler, ILapRFConnectionProvider connectionProvider, IServiceProvider serviceProvider)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _serviceProvider = serviceProvider ?? throw new ArgumentException("Connection factory must implement IServiceProvider to allow access to reaction provider.", nameof(connectionFactory));

    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IReactionProvider provider = _serviceProvider.GetRequiredService<IReactionProvider>();

        provider.On<Events.ImmersionRCLapRFNetworkConfigurationChanged>(async x =>
        {
            if (_hardwareProcesses.TryGetValue(x.TimerId, out ILapRFConnection? existingConnection))
            {
                await existingConnection.Stop();
                _hardwareProcesses.TryRemove(x.TimerId, out _);
            }

            if (IPAddress.TryParse(x.IPAddress, out IPAddress? ipAddress))
            {
                ILapRFConnection connection = _connectionFactory.CreateEthernetConnection(x.TimerId, ipAddress, x.Port);
                _connectionProvider.SetLapRFConnection(x.TimerId, connection);
                _hardwareProcesses[x.TimerId] = connection;
            }
        });

        provider.On<Events.ImmersionRCLapRFNetworkDeviceRegistered>(async x =>
        {
            if (_hardwareProcesses.TryGetValue(x.TimerId, out ILapRFConnection? existingConnection))
            {
                await existingConnection.Stop();
                _hardwareProcesses.TryRemove(x.TimerId, out _);
            }

            if (IPAddress.TryParse(x.IPAddress, out IPAddress? ipAddress))
            {
                ILapRFConnection connection = _connectionFactory.CreateEthernetConnection(x.TimerId, ipAddress, x.Port);
                _connectionProvider.SetLapRFConnection(x.TimerId, connection);
                _hardwareProcesses[x.TimerId] = connection;
            }
        });

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
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, ILapRFConnection> device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }
    }
}
