﻿using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Messages;
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

        _rpcRegistrar.RegisterHandler<Guid, IEnumerable<LapRFLaneConfiguration>>("GetEthernetLapRF8ChannelTimerLaneConfigurations", GetEthernetLapRF8ChannelTimerLaneConfigurations);
        _rpcRegistrar.RegisterHandler<Guid, byte, LapRFLaneConfiguration?>("GetEthernetLapRF8ChannelTimerLaneConfiguration", GetEthernetLapRF8ChannelTimerLaneConfiguration);
        _rpcRegistrar.RegisterHandler<Guid, bool>("IsEthernetLapRF8ChannelTimerConnected", IsEthernetLapRF8ChannelTimerConnected);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IEnumerable<EthernetLapRF8ChannelTimer>? response = await _rpcClient.InvokeAsync<IEnumerable<EthernetLapRF8ChannelTimer>>("GetAllEthernetLapRF8ChannelTimers");

        if (response == null)
        {
            return;
        }

        foreach (EthernetLapRF8ChannelTimer device in response)
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

    private async Task<IEnumerable<LapRFLaneConfiguration>> GetEthernetLapRF8ChannelTimerLaneConfigurations(Guid timerId)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return Enumerable.Empty<LapRFLaneConfiguration>();
        }

        if (!connection.IsConnected)
        {
            return Enumerable.Empty<LapRFLaneConfiguration>();
        }

        IEnumerable<NaeTime.Hardware.ImmersionRC.Models.LapRF8ChannelLaneConfiguration> frequencies = await connection.GetAllLaneConfigurations().ConfigureAwait(false);

        return frequencies.Select(x => new LapRFLaneConfiguration(x.Lane, x.BandId, x.FrequencyInMhz, x.IsEnabled, x.Gain, x.Threshold));
    }
    private async Task<LapRFLaneConfiguration?> GetEthernetLapRF8ChannelTimerLaneConfiguration(Guid timerId, byte laneId)
    {
        if (!_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection))
        {
            return null;
        }

        if (!connection.IsConnected)
        {
            return null;
        }

        IEnumerable<NaeTime.Hardware.ImmersionRC.Models.LapRF8ChannelLaneConfiguration> frequencies = await connection.GetLaneConfigurations(laneId).ConfigureAwait(false);

        if (!frequencies.Any())
        {
            return null;
        }

        NaeTime.Hardware.ImmersionRC.Models.LapRF8ChannelLaneConfiguration configuration = frequencies.First();

        return new LapRFLaneConfiguration(laneId, configuration.BandId, configuration.FrequencyInMhz, configuration.IsEnabled, configuration.Gain, configuration.Threshold);
    }

    private async Task<bool> IsEthernetLapRF8ChannelTimerConnected(Guid timerId)
        => !_hardwareProcesses.TryGetValue(timerId, out LapRFConnection? connection) ? false : connection.IsConnected;
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
}
