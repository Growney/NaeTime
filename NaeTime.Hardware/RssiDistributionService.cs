using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Hosting;
using NaeTime.Hardware.Abstractions;

namespace NaeTime.Hardware;

internal class RssiDistributionService : BackgroundService
{
    private readonly RssiChannel _rssiChannel;
    private readonly IEnumerable<IRssiConsumer> _rssiConsumers;

    public RssiDistributionService(RssiChannel rssiChannel, IEnumerable<IRssiConsumer> rssiConsumers)
    {
        _rssiChannel = rssiChannel ?? throw new ArgumentNullException(nameof(rssiChannel));
        _rssiConsumers = rssiConsumers ?? throw new ArgumentNullException(nameof(rssiConsumers));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while(!stoppingToken.IsCancellationRequested)
        {
            var rssiData = await _rssiChannel.ChannelQueue.WaitForDequeueAsync(stoppingToken);
            foreach (var consumer in _rssiConsumers)
            {
                consumer.HandleRssi(rssiData);
            }
        }
    }
}
