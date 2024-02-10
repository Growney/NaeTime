using Microsoft.Extensions.Hosting;
using NaeTime.Messages.Events.Hardware;
using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.ImmersionRC.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Timing.ImmersionRC.Hardware;
internal class LapRFManager : IHostedService
{
    private readonly IPublishSubscribe _publisher;
    private readonly ILapRFConnectionFactory _connectionFactory;

    private readonly ConcurrentDictionary<Guid, LapRFConnection> _hardwareProcesses = new();

    public LapRFManager(IPublishSubscribe publisher, ILapRFConnectionFactory connectionFactory)
    {
        _publisher = publisher ?? throw new ArgumentNullException(nameof(publisher));
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));

        _publisher.Subscribe<EthernetLapRF8ChannelConfigured>(this, When);
        _publisher.Subscribe<LaneEnabled>(this, When);
        _publisher.Subscribe<LaneDisabled>(this, When);
        _publisher.Subscribe<LaneRadioFrequencyConfigured>(this, When);
        _publisher.RespondTo<TimerLaneConfigurationRequest, TimerLaneConfigurationResponse>(this, On);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var response = await _publisher.Request<EthernetLapRF8ChannelTimersRequest, EthernetLapRF8ChannelTimersResponse>();

        if (response == null)
        {
            return;
        }

        foreach (var device in response.Timers)
        {
            var connection = _connectionFactory.CreateEthernetConnection(device.TimerId, device.IpAddress, device.Port);
            _hardwareProcesses.TryAdd(device.TimerId, connection);
        }
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var device in _hardwareProcesses)
        {
            await device.Value.Stop();
        }

        _publisher.Unsubscribe(this);
    }

    private async Task<TimerLaneConfigurationResponse?> On(TimerLaneConfigurationRequest requested)
    {
        if (!_hardwareProcesses.TryGetValue(requested.TimerId, out var connection))
        {
            return null;
        }

        if (!connection.IsConnected)
        {
            return null;
        }

        var frequencies = await connection.GetRadioFrequencyChannelsAsync();

        return new TimerLaneConfigurationResponse(requested.TimerId, frequencies.Select(x => new TimerLaneConfigurationResponse.TimerLaneConfiguration(x.Lane, x.BandId, x.FrequencyInMhz, x.IsEnabled)));
    }

    private async Task When(EthernetLapRF8ChannelConfigured configured)
    {
        if (_hardwareProcesses.TryGetValue(configured.TimerId, out var connection))
        {
            await connection.Stop();
        }

        var newConnection = _connectionFactory.CreateEthernetConnection(configured.TimerId, configured.IpAddress, configured.Port);

        _hardwareProcesses.AddOrUpdate(configured.TimerId, newConnection,
            (id, existing) => newConnection);
    }
    public async Task When(LaneEnabled lane)
    {
        var timers = _hardwareProcesses.Values;

        foreach (var timer in timers)
        {
            await timer.SetLaneStatus(lane.LaneNumber, true);
        }

    }
    public async Task When(LaneDisabled lane)
    {
        var timers = _hardwareProcesses.Values;

        foreach (var timer in timers)
        {
            await timer.SetLaneStatus(lane.LaneNumber, true);
        }
    }
    public async Task When(LaneRadioFrequencyConfigured lane)
    {
        var timers = _hardwareProcesses.Values;

        foreach (var timer in timers)
        {
            await timer.SetLaneRadioFrequency(lane.LaneNumber, lane.FrequencyInMhz);
        }
    }

}
