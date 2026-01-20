using EventDbLite.Reactions.Abstractions;
using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Query.Abstractions;
using System.Collections.Concurrent;
using System.Net;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeManager : BackgroundService
{
    private readonly INodeConnectionFactory _connectionFactory;
    private readonly IHardwareQueryHandler _queryHandler;
    private readonly INodeConnectionProvider _connectionProvider;
    private readonly IReactionProviderFactory _reactionProviderFactory;

    private readonly ConcurrentDictionary<Guid, INodeConnection> _hardwareProcesses = new();

    public NodeManager(INodeConnectionFactory connectionFactory, IHardwareQueryHandler queryHandler, INodeConnectionProvider connectionProvider, IReactionProviderFactory reactionProviderFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _connectionProvider = connectionProvider ?? throw new ArgumentNullException(nameof(connectionProvider));
        _reactionProviderFactory = reactionProviderFactory ?? throw new ArgumentException("Connection factory must implement IServiceProvider to allow access to reaction provider.", nameof(connectionFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IEnumerable<Query.Abstractions.Models.NaeTimeNode> devices = await _queryHandler.GetAllNaeTimeNodes();

        foreach (Query.Abstractions.Models.NaeTimeNode device in devices)
        {
            INodeConnection connection = device switch
            {
                NaeTime.Query.Abstractions.Models.NetworkNaeTimeNode ethernetDevice => _connectionFactory.CreateTcpConnection(ethernetDevice.Id, System.Net.IPAddress.Parse(ethernetDevice.IPAddress), ethernetDevice.Port),
                _ => throw new NotSupportedException($"NaeTime node device type {device.GetType().FullName} is not supported.")
            };

            _connectionProvider.SetNodeConnection(device.Id, connection);
            _hardwareProcesses[device.Id] = connection;
            await connection.Start();
        }

        await Task.WhenAll(
            _reactionProviderFactory.On<Events.NaeTimeNodeNetworkConfigurationChanged>(HandleNetworkConfigurationChange, stoppingToken),
            _reactionProviderFactory.On<Events.NaeTimeNodeNetworkDeviceRegistered>(HandleNetworkDeviceRegistered, stoppingToken));
    }

    private async Task HandleNetworkConfigurationChange(Events.NaeTimeNodeNetworkConfigurationChanged e)
    {
        if (_hardwareProcesses.TryGetValue(e.TimerId, out INodeConnection? existingConnection))
        {
            await existingConnection.Stop();
            _hardwareProcesses.TryRemove(e.TimerId, out _);
        }
        if (IPAddress.TryParse(e.IPAddress, out IPAddress? ipAddress))
        {
            INodeConnection connection = _connectionFactory.CreateTcpConnection(e.TimerId, ipAddress, e.Port);
            _connectionProvider.SetNodeConnection(e.TimerId, connection);
            _hardwareProcesses[e.TimerId] = connection;
            await connection.Start();
        }
    }

    private async Task HandleNetworkDeviceRegistered(Events.NaeTimeNodeNetworkDeviceRegistered e)
    {
        if (_hardwareProcesses.TryGetValue(e.TimerId, out INodeConnection? existingConnection))
        {
            await existingConnection.Stop();
            _hardwareProcesses.TryRemove(e.TimerId, out _);
        }
        if (IPAddress.TryParse(e.IPAddress, out IPAddress? ipAddress))
        {
            INodeConnection connection = _connectionFactory.CreateTcpConnection(e.TimerId, ipAddress, e.Port);
            _connectionProvider.SetNodeConnection(e.TimerId, connection);
            _hardwareProcesses[e.TimerId] = connection;
            await connection.Start();
        }
    }
}