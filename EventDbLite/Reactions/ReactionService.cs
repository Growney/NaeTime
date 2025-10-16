using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Reactions;
public class ReactionService(IServiceProvider serviceProvider) : IHostedService
{
    private const string ReactionStreamName = "$reactions";
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    private IReactionProvider? _reactionProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task _completionTask = Task.CompletedTask;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IEnumerable<ConstantReactionSource> reactionSources = _serviceProvider.GetServices<ConstantReactionSource>();

        Dictionary<Type, List<Func<IServiceProvider, object, Task>>> reactionMap = new();

        foreach (ConstantReactionSource reactionSource in reactionSources)
        {
            foreach (ConstantReaction reaction in reactionSource.Reactions)
            {
                if (!reactionMap.TryGetValue(reaction.TargetType, out var handlers))
                {
                    handlers = new List<Func<IServiceProvider, object, Task>>();
                    reactionMap[reaction.TargetType] = handlers;
                }
                handlers.Add(reaction.Handler);
            }
        }

        List<Task> reactionTasks = new();
        foreach (var kvp in reactionMap)
        {
            reactionTasks.Add(StreamEventType(kvp.Key, kvp.Value, _cancellationTokenSource.Token));
        }

        _completionTask = Task.WhenAll(reactionTasks);

        return Task.CompletedTask;
    }

    private async Task StreamEventType(Type targetType, IEnumerable<Func<IServiceProvider, object, Task>> handlers, CancellationToken token)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IReactionProviderFactory factory = scope.ServiceProvider.GetRequiredService<IReactionProviderFactory>();

        StreamPosition position = await GetReactionPosition(targetType, scope.ServiceProvider);

        IReactionProvider provider = factory.CreateProvider(position);

        await foreach (ReactionEvent streamEvent in provider.StreamSubscription(targetType, token))
        {
            using IServiceScope eventScope = scope.ServiceProvider.CreateScope();

            foreach (Func<IServiceProvider, object, Task> handler in handlers)
            {
                await handler(eventScope.ServiceProvider, streamEvent.Payload);
            }

            await StoreReactionPosition(targetType, streamEvent.SubscriptionEvent.Event, eventScope.ServiceProvider);
        }
    }
    private static string GetStreamName(Type targetType) => $"$reactions-{targetType.FullName}";
    private static async Task<StreamPosition> GetReactionPosition(Type targetType, IServiceProvider services)
    {
        IEventSerializer _eventSerializer = services.GetRequiredService<IEventSerializer>();
        IEventStoreLite _eventStreamConnection = services.GetRequiredService<IEventStoreLite>();
        string reactionEventIdentifier = _eventSerializer.GetIdentifier(typeof(ReactionHandled));

        await foreach (StreamEvent streamEvent in _eventStreamConnection.ReadStreamEvents(GetStreamName(targetType), StreamDirection.Reverse, StreamPosition.End))
        {
            EventMetadata metadata = _eventSerializer.DeserializeMetadata(streamEvent.Data.Metadata);

            if (metadata.Identifier != reactionEventIdentifier)
            {
                continue;
            }

            ReactionHandled? handled = _eventSerializer.DeserializeEvent(streamEvent.Data.Payload, typeof(ReactionHandled)) as ReactionHandled;

            if (handled is null)
            {
                continue;
            }

            return StreamPosition.WithGlobalVersion(handled.GlobalOrdinal);
        }

        return StreamPosition.Beginning;
    }
    private static Task StoreReactionPosition(Type targetType, StreamEvent streamEvent, IServiceProvider services)
    {
        IStreamEventWriter _streamEventWriter = services.GetRequiredService<IStreamEventWriter>();

        ReactionHandled handledEvent = new()
        {
            GlobalOrdinal = streamEvent.GlobalOrdinal,
        };

        return _streamEventWriter.AppendToStream(GetStreamName(targetType), handledEvent);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        if (_reactionProvider != null)
        {
            await _reactionProvider.DisposeAsync();
        }

        await _completionTask;
    }
}
