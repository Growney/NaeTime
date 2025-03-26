using Microsoft.Extensions.DependencyInjection;
using NaeTime.Orchestrator.Distribution.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NaeTime.Orchestrator.Distribution.Channels;

public class ChannelsDistributionReceiver : IDistributionReceiver
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ConcurrentDictionary<Type, object> _activeChannels = new();

    public ChannelsDistributionReceiver(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async ValueTask<T?> WaitForNextAsync<T>(CancellationToken token)
    {
        object unboxedChannel = _activeChannels.GetOrAdd(typeof(T), _ => _serviceProvider.GetRequiredService<Channel<T>>());
        if (unboxedChannel is not Channel<T> channel)
        {
            throw new InvalidOperationException("Channel was not of the expected type");
        }

        if (!await channel.Reader.WaitToReadAsync(token))
        {
            return default;
        }

        return await channel.Reader.ReadAsync(token);
    }
}
