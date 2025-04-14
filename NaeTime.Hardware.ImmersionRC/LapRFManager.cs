using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions.Events.Hardware;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Timing.ImmersionRC;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.ImmersionRC;
internal class LapRFManager : IHostedService, ILapRFManager, IDisposable
{
    private readonly INaeTimeOrchestratorPersistence _orchestrator;
    private readonly ILapRFConnectionFactory _connectionFactory;
    private readonly CancellationTokenSource _source = new();

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(INaeTimeOrchestratorPersistence orchestrator, ILapRFConnectionFactory connectionFactory, IDistributionReceiver receiver)
    {
        _orchestrator = orchestrator ?? throw new ArgumentNullException(nameof(orchestrator));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _ = receiver.Process<EthernetLapRF8Created>(_source.Token, When);
        _ = receiver.Process<EthernetLapRF8Configured>(_source.Token, When);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Persistence.Abstractions.Hardware.EthernetLapRF8ChannelTimer> ethernetTimers = await _orchestrator.Hardware.GetAllEthernetLapRF8ChannelTimers();

        if (ethernetTimers == null)
        {
            return;
        }

        foreach (Persistence.Abstractions.Hardware.EthernetLapRF8ChannelTimer device in ethernetTimers)
        {
            LapRFConnection connection = _connectionFactory.CreateEthernetConnection(device.TimerId, device.IpAddress, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (KeyValuePair<Guid, LapRFConnection> device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }
    }
    public async Task When(EthernetLapRF8Configured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.Id, out LapRFConnection? connection))
        {
            await connection.Stop().ConfigureAwait(false);
        }

        LapRFConnection newConnection = _connectionFactory.CreateEthernetConnection(configured.Id, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.Id, newConnection,
            (id, existing) => newConnection);
    }
    public async Task When(EthernetLapRF8Created configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.Id, out LapRFConnection? connection))
        {
            await connection.Stop().ConfigureAwait(false);
        }

        LapRFConnection newConnection = _connectionFactory.CreateEthernetConnection(configured.Id, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.Id, newConnection,
            (id, existing) => newConnection);
    }

    public async Task<LapRFLaneConfiguration?> GetTimerLaneConfiguration(Guid timerId, byte laneId)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return null;
        }

        if (!connection.IsConnected)
        {
            return null;
        }

        IEnumerable<LapRFLaneConfiguration> configs = await connection.GetLaneConfigurations(laneId).ConfigureAwait(false);

        return configs.FirstOrDefault();
    }
    public async Task<IEnumerable<LapRFLaneConfiguration>> GetTimeLaneConfigurations(Guid timerId) => !_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection)
            ? Enumerable.Empty<LapRFLaneConfiguration>()
            : !connection.IsConnected
            ? Enumerable.Empty<LapRFLaneConfiguration>()
            : await connection.GetAllLaneConfigurations().ConfigureAwait(false);

    public Task<bool> IsTimerConnected(Guid timerId) => Task.FromResult(_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection) && connection.IsConnected);
    public void Dispose() => _source.Cancel();
    public async Task<bool> ConfigureLaneStatus(Guid timerId, byte laneId, bool isEnabled)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return false;
        }

        await connection.SetLaneStatus(laneId, isEnabled);
        return true;
    }
    public async Task<bool> ConfigureLaneRadioFrequency(Guid timerId, byte laneId, byte? bandId, int frequencyInMhz)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return false;
        }

        await connection.SetLaneRadioFrequency(laneId, frequencyInMhz);
        return true;
    }
    public async Task<bool> ConfigureLaneGain(Guid timerId, byte laneId, ushort gain)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return false;
        }

        await connection.SetLaneGain(laneId, gain);
        return true;
    }
    public async Task<bool> ConfigureLaneThreshold(Guid timerId, byte laneId, float threshold)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return false;
        }

        await connection.SetLaneThreshold(laneId, threshold);
        return true;
    }
}
