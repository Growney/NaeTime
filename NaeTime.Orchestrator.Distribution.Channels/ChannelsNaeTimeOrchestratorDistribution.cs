using Microsoft.Extensions.DependencyInjection;
using NaeTime.Orchestrator.Distribution.Abstractions;
using System.Collections.Concurrent;
using System.Threading.Channels;

namespace NaeTime.Orchestrator.Distribution.Channels;

public class ChannelsNaeTimeOrchestratorDistribution : INaeTimeOrchestratorDistribution
{

    private readonly ConcurrentQueue<Func<CancellationToken, ValueTask>> _queue = new();
    private readonly IServiceProvider _serviceProvider;
    public ChannelsNaeTimeOrchestratorDistribution(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public async Task CommitAsync()
    {
        while (_queue.TryDequeue(out Func<CancellationToken, ValueTask>? action))
        {
            if (action != null)
            {
                await action(CancellationToken.None);
            }
        }
    }
    public Task Distribute<T>(T message)
    {
        _queue.Enqueue(async token =>
        {
            Channel<T> channel = _serviceProvider.GetRequiredService<Channel<T>>();
            if (await channel.Writer.WaitToWriteAsync(token))
            {
                await channel.Writer.WriteAsync(message, token);
            }
        });

        return Task.CompletedTask;
    }
}
