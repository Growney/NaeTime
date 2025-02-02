using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Messages;
using NaeTime.Hardware.Messages.Models;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeManager : IHostedService
{
    private readonly IRemoteProcedureCallClient _rpcClient;
    private readonly IEventRegistrarScope _eventRegistrarScope;
    private readonly IRemoteProcedureCallRegistrar _rpcRegistrar;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IEventClient _eventClient;
    private readonly INodeConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, NodeConnection> _hardwareProcesses = new();

    public NodeManager(INodeConnectionFactory connectionFactory, IRemoteProcedureCallClient rpcClient, IEventRegistrarScope eventRegistrarScope, IRemoteProcedureCallRegistrar rpcRegistrar, ISoftwareTimer softwareTimer, IEventClient eventClient)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        _eventRegistrarScope = eventRegistrarScope ?? throw new ArgumentNullException(nameof(eventRegistrarScope));
        _rpcRegistrar = rpcRegistrar ?? throw new ArgumentNullException(nameof(rpcRegistrar));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));

        _eventRegistrarScope.RegisterHub(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<SerialEsp32Node>? response = await _rpcClient.InvokeAsync<IEnumerable<SerialEsp32Node>>("GetAllSerialEsp32NodeTimers");

        if (response == null)
        {
            return;
        }

        foreach (SerialEsp32Node device in response)
        {
            NodeConnection connection = _connectionFactory.CreateSerialConnection(device.TimerId, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, NodeConnection> device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }

        _eventRegistrarScope.Dispose();
    }

    public Task When(NodeTimerLaneRadioFrequencyConfigured lane)
    => !_hardwareProcesses.TryGetValue(lane.TimerId, out NodeConnection? connection)
        ? Task.CompletedTask
        : !connection.IsConnected
            ? Task.CompletedTask
            : connection.SetLaneRadioFrequency(lane.Lane, lane.FrequencyInMhz);

    public Task When(NodeTimerEntryThresholdConfigured threshold)
    => !_hardwareProcesses.TryGetValue(threshold.TimerId, out NodeConnection? connection)
        ? Task.CompletedTask
        : !connection.IsConnected
            ? Task.CompletedTask
            : connection.SetLaneEntryThreshold(threshold.Lane, threshold.Threshold);

    public Task When(NodeTimerExitThresholdConfigured threshold)
     => !_hardwareProcesses.TryGetValue(threshold.TimerId, out NodeConnection? connection)
        ? Task.CompletedTask
        : !connection.IsConnected
            ? Task.CompletedTask
            : connection.SetLaneExitThreshold(threshold.Lane, threshold.Threshold);
}