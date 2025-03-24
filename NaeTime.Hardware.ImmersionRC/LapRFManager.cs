using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;
using NaeTime.Hardware.Messages;
using NaeTime.Persistence.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.ImmersionRC;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.ImmersionRC;
internal class LapRFManager : IHostedService, ILapRFManager
{
    private readonly INaeTimePersistence _persistence;
    private readonly IEventRegistrarScope _eventRegistrarScope;
    private readonly ILapRFConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(INaeTimePersistence persistence, IEventRegistrarScope eventRegistrarScope, ILapRFConnectionFactory connectionFactory)
    {
        _persistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _eventRegistrarScope = eventRegistrarScope ?? throw new ArgumentNullException(nameof(eventRegistrarScope));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _eventRegistrarScope.RegisterHub(this);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<Persistence.Abstractions.Hardware.EthernetLapRF8ChannelTimer> ethernetTimers = await _persistence.Hardware.GetAllEthernetLapRF8ChannelTimers();

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

        _eventRegistrarScope.Dispose();
    }
    public async Task When(EthernetLapRF8ChannelConfigured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.TimerId, out LapRFConnection? connection))
        {
            await connection.Stop().ConfigureAwait(false);
        }

        LapRFConnection newConnection = _connectionFactory.CreateEthernetConnection(configured.TimerId, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.TimerId, newConnection,
            (id, existing) => newConnection);
    }
    public Task When(EthernetLapRF8ChannelTimerLaneEnabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out LapRFConnection? connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneStatus(lane.Lane, true);
    public Task When(EthernetLapRF8ChannelTimerLaneDisabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out LapRFConnection? connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneStatus(lane.Lane, false);
    public Task When(EthernetLapRF8ChannelTimerLaneRadioFrequencyConfigured lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out LapRFConnection? connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneRadioFrequency(lane.Lane, lane.FrequencyInMhz);
    public Task When(EthernetLapRF8ChannelTimerLaneThresholdConfigured lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out LapRFConnection? connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneThreshold(lane.Lane, lane.Threshold);
    public Task When(EthernetLapRF8ChannelTimerLaneGainConfigured lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out LapRFConnection? connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneGain(lane.Lane, lane.Gain);
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
}
