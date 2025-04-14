using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Hardware;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeManager : IHostedService, INodeManager
{
    private readonly INaeTimeOrchestratorPersistence _orchestrator;
    private readonly INodeConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, NodeConnection> _hardwareProcesses = new();

    public NodeManager(INodeConnectionFactory connectionFactory, INaeTimeOrchestratorPersistence orchestrator)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
    }

    public async Task<bool> ConfigureLaneEntryThreshold(Guid timerId, byte laneId, ushort threshold)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out NodeConnection? connection))
        {
            return false;
        }

        if (!connection.IsConnected)
        {
            return false;
        }

        await connection.SetLaneEntryThreshold(laneId, threshold);

        return true;
    }
    public async Task<bool> ConfigureLaneExitThreshold(Guid timerId, byte laneId, ushort threshold)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out NodeConnection? connection))
        {
            return false;
        }

        if (!connection.IsConnected)
        {
            return false;
        }

        await connection.SetLaneEntryThreshold(laneId, threshold);

        return true;
    }
    public async Task<bool> ConfigureLaneRadioFrequency(Guid timerId, byte laneId, byte? bandId, int frequencyInMhz)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out NodeConnection? connection))
        {
            return false;
        }

        if (!connection.IsConnected)
        {
            return false;
        }

        await connection.SetLaneRadioFrequency(laneId, frequencyInMhz);

        return true;
    }
    public async Task<bool> ConfigureLaneStatus(Guid timerId, byte laneId, bool isEnabled)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out NodeConnection? connection))
        {
            return false;
        }

        if (!connection.IsConnected)
        {
            return false;
        }

        await connection.SetLaneEnabled(laneId, isEnabled);

        return true;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<SerialEsp32Node>? response = await _orchestrator.Hardware.GetAllSerialEsp32NodeTimers();

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
    }
}