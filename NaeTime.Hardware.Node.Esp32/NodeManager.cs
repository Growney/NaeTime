using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Messages;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using NaeTime.PubSub.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeManager : IHostedService
{
    private readonly INaeTimePersistence _persistence;
    private readonly IEventRegistrarScope _eventRegistrarScope;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IEventClient _eventClient;
    private readonly INodeConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, NodeConnection> _hardwareProcesses = new();

    public NodeManager(INodeConnectionFactory connectionFactory, INaeTimePersistence persistence, IEventRegistrarScope eventRegistrarScope, ISoftwareTimer softwareTimer, IEventClient eventClient)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _eventRegistrarScope = eventRegistrarScope ?? throw new ArgumentNullException(nameof(eventRegistrarScope));
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));

        _eventRegistrarScope.RegisterHub(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<SerialEsp32Node>? response = await _persistence.Hardware.GetAllSerialEsp32NodeTimers();

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

    public Task When(NodeTimerLaneEnabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out NodeConnection? connection)
        ? Task.CompletedTask
        : !connection.IsConnected
            ? Task.CompletedTask
            : connection.SetLaneEnabled(lane.Lane, true);
    public Task When(NodeTimerLaneDisabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out NodeConnection? connection)
        ? Task.CompletedTask
        : !connection.IsConnected
            ? Task.CompletedTask
            : connection.SetLaneEnabled(lane.Lane, false);
}