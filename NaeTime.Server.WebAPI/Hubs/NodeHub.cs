using Microsoft.AspNetCore.SignalR;
using NaeTime.Node.Abstractions.Enumeration;
using NaeTime.Node.WebAPI.Shared.Models;
using NaeTime.Server.Abstractions.Consumers;
using NaeTime.Server.Abstractions.Events;

namespace NaeTime.Server.WebAPI.Hubs;

public class NodeHub : Hub
{
    private readonly int _threshold = 1500;

    private readonly ILogger<NodeHub> _logger;
    private readonly INodeConsumer _readingConsumer;

    public NodeHub(INodeConsumer readingConsumer, ILogger<NodeHub> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _readingConsumer = readingConsumer;

    }

    public override Task OnConnectedAsync()
    {
        return base.OnConnectedAsync();
    }
    public async Task RssiReadingGroup(RssiReadingGroupDto readingGroup)
    {

        try
        {
            var receivedEvent = new RssiReadingGroupReceived()
            {
                GroupId = Guid.NewGuid(),
                NodeId = readingGroup.NodeId,
                DeviceId = readingGroup.DeviceId,
                FrequencyId = readingGroup.Frequency
            };

            await _readingConsumer.When(receivedEvent, GenerateReadingEvents(receivedEvent.GroupId, readingGroup));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error writing reading group");
        }

    }

    private IEnumerable<RssiReadingReceived> GenerateReadingEvents(Guid groupId, RssiReadingGroupDto readingGroup)
    {
        foreach (var reading in readingGroup.Readings)
        {
            yield return new RssiReadingReceived()
            {
                ReadingGroupId = groupId,
                Tick = reading.Tick,
                Value = reading.Value
            };
        }
    }

    public Task NodeConfigured(NodeConfigurationDto nodeConfig)
    {
        var configuredEvent = new NodeConfigured()
        {
            NodeId = nodeConfig.NodeId,
            ServerAddress = nodeConfig.ServerAddress,
            AnalogToDigitalConverters = nodeConfig.AnalogToDigitalConverters.Select(x => new NodeConfigured.AnalogToDigitalConverterConfiguration()
            {
                Id = x.Id,
                ChipSelectLine = x.ChipSelectLine,
                BusId = x.BusId,
                ClockFrequency = x.ClockFrequency,
                DataBitLength = x.DataBitLength,
                SpiMode = x.SpiMode switch
                {
                    Node.WebAPI.Shared.Enumeration.SpiModeDto.Mode0 => Abstractions.Events.NodeConfigured.AnalogToDigitalConverterConfiguration.ADCSpiMode.Mode0,
                    Node.WebAPI.Shared.Enumeration.SpiModeDto.Mode1 => Abstractions.Events.NodeConfigured.AnalogToDigitalConverterConfiguration.ADCSpiMode.Mode1,
                    Node.WebAPI.Shared.Enumeration.SpiModeDto.Mode2 => Abstractions.Events.NodeConfigured.AnalogToDigitalConverterConfiguration.ADCSpiMode.Mode2,
                    Node.WebAPI.Shared.Enumeration.SpiModeDto.Mode3 => Abstractions.Events.NodeConfigured.AnalogToDigitalConverterConfiguration.ADCSpiMode.Mode3,
                    _ => throw new NotImplementedException()
                },
                Mode = x.Mode switch
                {
                    AnalogToDigitalConverterModeDto.HardwareSPI => Abstractions.Events.NodeConfigured.AnalogToDigitalConverterConfiguration.AnalogToDigitalConverterModeDto.HardwareSPI,
                    _ => throw new NotImplementedException()
                }

            }),
            MultiplexedRX5808Configurations = nodeConfig.MultiplexedRX5808Configurations.Select(x => new NodeConfigured.MultiplexedAnalogToDigitalConverterRx5808Configuration()
            {
                Id = x.Id,
                ADCChannel = x.ADCChannel,
                ADCId = x.ADCId,
                Frequency = x.Frequency,
                IsEnabled = x.IsEnabled,
                MultiplexerChannel = x.MultiplexerChannel,
                MultiplexerId = x.MultiplexerId,
            }),
            MultiplexerConfigurations = nodeConfig.MultiplexerConfigurations.Select(x => new NodeConfigured.MultiplexerConfiguration()
            {
                Id = x.Id,
                SelectPin = x.SelectPin,
                AAddressPin = x.AAddressPin,
                BAddressPin = x.BAddressPin,
                CAddressPin = x.CAddressPin,
                DataPin = x.DataPin,
                ClockPin = x.ClockPin,
            }),
            RX5808Configurations = nodeConfig.RX5808Configurations.Select(x => new NodeConfigured.AnalogToDigitalConverterRX5808Configuration()
            {
                Id = x.Id,
                DataPin = x.DataPin,
                SelectPin = x.SelectPin,
                ClockPin = x.ClockPin,
                ADCId = x.ADCId,
                ADCChannel = x.ADCChannel,
                Frequency = x.Frequency,
                IsEnabled = x.IsEnabled,
            })
        };

        _readingConsumer.When(configuredEvent);

        _logger.LogInformation("Node configured {nodeId}", nodeConfig.NodeId);
        return Task.CompletedTask;
    }

    public Task TimerStarted(Guid nodeId, Guid sessionId)
    {
        _readingConsumer.When(new NodeTimerStarted()
        {
            NodeId = nodeId,
            SessionId = sessionId
        });
        _logger.LogInformation("Node timer started {nodeId} - session {sessionId}", nodeId, sessionId);
        return Task.CompletedTask;
    }
    public Task TimerStopped(Guid nodeId, Guid sessionId)
    {
        _readingConsumer.When(new NodeTimerStopped()
        {
            NodeId = nodeId,
            SessionId = sessionId
        });
        _logger.LogInformation("Node timer stopped {nodeId} - session {sessionId}", nodeId, sessionId);
        return Task.CompletedTask;
    }
}
