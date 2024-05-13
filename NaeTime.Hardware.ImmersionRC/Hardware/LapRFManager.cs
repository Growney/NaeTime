using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Messages.Messages;
using NaeTime.Hardware.Messages.Models;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : IHostedService
{
    private readonly IRemoteProcedureCallClient _rpcClient;
    private readonly IEventRegistrarScope _eventRegistrarScope;
    private readonly IRemoteProcedureCallRegistrar _rpcRegistrar;
    private readonly ILapRFConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(IRemoteProcedureCallClient rpcClient, IEventRegistrarScope eventRegistrarScope, IRemoteProcedureCallRegistrar rpcRegistrar, ILapRFConnectionFactory connectionFactory)
    {
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
        _eventRegistrarScope = eventRegistrarScope ?? throw new ArgumentNullException(nameof(eventRegistrarScope));
        _rpcRegistrar = rpcRegistrar ?? throw new ArgumentNullException(nameof(rpcRegistrar));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _eventRegistrarScope.RegisterHub(this);

        _rpcRegistrar.RegisterHandler<Guid, IEnumerable<EthernetLapRF8ChannelTimerLaneConfiguration>>("GetEthernetLapRF8ChannelTimerLaneConfigurations", GetEthernetLapRF8ChannelTimerLaneConfigurations);
        _rpcRegistrar.RegisterHandler<Guid, byte, EthernetLapRF8ChannelTimerLaneConfiguration?>("GetEthernetLapRF8ChannelTimerLaneConfiguration", GetEthernetLapRF8ChannelTimerLaneConfiguration);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var response = await _rpcClient.InvokeAsync<IEnumerable<EthernetLapRF8ChannelTimer>>("GetAllEthernetLapRF8ChannelTimers");

        if (response == null)
        {
            return;
        }

        foreach (var device in response)
        {
            var connection = _connectionFactory.CreateEthernetConnection(device.TimerId, device.IpAddress, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in _hardwareProcesses)
        {
            await device.Value.Stop().ConfigureAwait(false);
        }

        _eventRegistrarScope.Dispose();
    }

    private async Task<IEnumerable<EthernetLapRF8ChannelTimerLaneConfiguration>> GetEthernetLapRF8ChannelTimerLaneConfigurations(Guid timerId)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out var connection))
        {
            return Enumerable.Empty<EthernetLapRF8ChannelTimerLaneConfiguration>();
        }

        if (!connection.IsConnected)
        {
            return Enumerable.Empty<EthernetLapRF8ChannelTimerLaneConfiguration>();
        }

        var frequencies = await connection.GetAllLaneConfigurations().ConfigureAwait(false);

        return frequencies.Select(x => new EthernetLapRF8ChannelTimerLaneConfiguration(x.Lane, x.BandId, x.FrequencyInMhz, x.IsEnabled));
    }
    private async Task<EthernetLapRF8ChannelTimerLaneConfiguration?> GetEthernetLapRF8ChannelTimerLaneConfiguration(Guid timerId, byte laneId)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out var connection))
        {
            return null;
        }

        if (!connection.IsConnected)
        {
            return null;
        }

        var frequencies = await connection.GetLaneConfigurations(laneId).ConfigureAwait(false);

        if (!frequencies.Any())
        {
            return null;
        }

        var configuration = frequencies.First();

        return new EthernetLapRF8ChannelTimerLaneConfiguration(laneId, configuration.BandId, configuration.FrequencyInMhz, configuration.IsEnabled);
    }
    public async Task When(EthernetLapRF8ChannelConfigured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.TimerId, out var connection))
        {
            await connection.Stop().ConfigureAwait(false);
        }

        var newConnection = _connectionFactory.CreateEthernetConnection(configured.TimerId, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.TimerId, newConnection,
            (id, existing) => newConnection);
    }
    public Task When(EthernetLapRF8ChannelTimerLaneEnabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out var connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneStatus(lane.Lane, true);
    public Task When(EthernetLapRF8ChannelTimerLaneDisabled lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out var connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneStatus(lane.Lane, false);
    public Task When(EthernetLapRF8ChannelTimerLaneRadioFrequencyConfigured lane)
        => !_hardwareProcesses.TryGetValue(lane.TimerId, out var connection)
            ? Task.CompletedTask
            : !connection.IsConnected
                ? Task.CompletedTask
                : connection.SetLaneRadioFrequency(lane.Lane, lane.FrequencyInMhz);
}
