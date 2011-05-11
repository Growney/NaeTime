using ELRS.Backpack;
using EventDbLite.Reactions.Abstractions;
using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.ELRS.Abstractions;
using NaeTime.Query.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.ELRS;

internal class BackpackManager : BackgroundService
{
    private readonly IBackpackConnectionFactory _connectionFactory;
    private readonly IHardwareQueryHandler _queryHandler;
    private readonly IBackpackConnectorProvider _connectorProvider;
    private readonly IReactionProviderFactory _reactionProviderFactory;

    private readonly ConcurrentDictionary<Guid, IBackpackConnector> _backpackConnectors = new();

    public BackpackManager(IBackpackConnectionFactory connectionFactory, IHardwareQueryHandler queryHandler, IBackpackConnectorProvider connectorProvider, IReactionProviderFactory reactionProviderFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _queryHandler = queryHandler ?? throw new ArgumentNullException(nameof(queryHandler));
        _connectorProvider = connectorProvider ?? throw new ArgumentNullException(nameof(connectorProvider));
        _reactionProviderFactory = reactionProviderFactory ?? throw new ArgumentNullException(nameof(reactionProviderFactory));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IEnumerable<Query.Abstractions.Models.SerialELRSBackpackInterface> backpacks = await _queryHandler.GetAllSerialELRSBackpackInterfaces();

        foreach (Query.Abstractions.Models.SerialELRSBackpackInterface backpack in backpacks)
        {
            IBackpackConnection connection = _connectionFactory.Create(backpack.ComPort);
            BackpackConnector connector = new(backpack.Id, connection);

            _connectorProvider.SetBackpackConnector(backpack.Id, connector);
            _backpackConnectors[backpack.Id] = connector;

            await connector.Start();
        }

        await Task.WhenAll(
            _reactionProviderFactory.On<Events.ELRSBackpackInterfaceAdded>(HandleBackpackAdded, stoppingToken),
            _reactionProviderFactory.On<Events.ELRSBackpackInterfaceComPortReconfigured>(HandleComPortReconfigured, stoppingToken));
    }

    private async Task HandleBackpackAdded(Events.ELRSBackpackInterfaceAdded e)
    {
        if (_backpackConnectors.TryGetValue(e.Id, out IBackpackConnector? existingConnector))
        {
            await existingConnector.Stop();
            _backpackConnectors.TryRemove(e.Id, out _);
        }

        IBackpackConnection connection = _connectionFactory.Create(e.ComPort);
        BackpackConnector connector = new(e.Id, connection);

        _connectorProvider.SetBackpackConnector(e.Id, connector);
        _backpackConnectors[e.Id] = connector;

        await connector.Start();
    }

    private async Task HandleComPortReconfigured(Events.ELRSBackpackInterfaceComPortReconfigured e)
    {
        if (_backpackConnectors.TryGetValue(e.Id, out IBackpackConnector? existingConnector))
        {
            await existingConnector.Stop();
            _backpackConnectors.TryRemove(e.Id, out _);
        }

        IBackpackConnection connection = _connectionFactory.Create(e.ComPort);
        BackpackConnector connector = new(e.Id, connection);

        _connectorProvider.SetBackpackConnector(e.Id, connector);
        _backpackConnectors[e.Id] = connector;

        await connector.Start();
    }
}
