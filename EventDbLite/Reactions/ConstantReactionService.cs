using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Reactions;
public class ConstantReactionService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    private CancellationTokenSource? _cancellationTokenSource;
    private Task _completionTask = Task.CompletedTask;

    public ConstantReactionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IEnumerable<ConstantReactionSource> reactionSources = _serviceProvider.GetServices<ConstantReactionSource>();

        Dictionary<string, List<ConstantReaction>> reactionMap = new();

        foreach (ConstantReactionSource reactionSource in reactionSources)
        {
            string reactionKey = reactionSource.ReactionKey ?? "default-reactions";

            if (!reactionMap.TryGetValue(reactionKey, out var reactions))
            {
                reactions = new List<ConstantReaction>();
                reactionMap.Add(reactionKey, reactions);
            }

            foreach (ConstantReaction reaction in reactionSource.Reactions)
            {
                reactions.Add(reaction);
            }
        }

        List<Task> reactionTasks = new();
        foreach (var kvp in reactionMap)
        {
            reactionTasks.Add(StreamReactions(kvp.Key, kvp.Value, _cancellationTokenSource.Token));
        }

        _completionTask = Task.WhenAll(reactionTasks);

        return Task.CompletedTask;
    }

    private async Task StreamReactions(string reactionKey, IEnumerable<ConstantReaction> handlers, CancellationToken token)
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        IEventStoreLite store = scope.ServiceProvider.GetRequiredService<IEventStoreLite>();

        StreamPosition position = await GetReactionPosition(reactionKey, scope.ServiceProvider);

        IStreamSubscription subscription = store.SubscribeToAllStreams(position);

        IEventSerializer serializer = scope.ServiceProvider.GetRequiredService<IEventSerializer>();
        Dictionary<string, Dictionary<Type, List<ConstantReaction>>> identifiedReactions = GroupReactions(handlers, serializer);
        await foreach (SubscriptionEvent streamEvent in subscription.StreamEvents(token))
        {
            EventMetadata metadata = serializer.DeserializeMetadata(streamEvent.Event.Data.Metadata);

            if (!identifiedReactions.TryGetValue(metadata.Identifier, out var eventHandlers))
            {
                continue;
            }

            bool handledAny = false;
            foreach (var kvp in eventHandlers)
            {
                object? eventObject = serializer.DeserializeEvent(streamEvent.Event.Data.Payload, kvp.Key);

                if (eventObject is null)
                {
                    continue;
                }

                using IServiceScope eventScope = scope.ServiceProvider.CreateScope();

                foreach (ConstantReaction handler in kvp.Value)
                {
                    try
                    {
                        await handler.Handler(eventScope.ServiceProvider, eventObject);
                        handledAny = true;
                    }
                    catch
                    {
                        //TODO do something with the exception
                    }
                }
            }
            if (handledAny)
            {
                await StoreReactionPosition(reactionKey, streamEvent.Event, scope.ServiceProvider);
            }
        }
    }

    private static Dictionary<string, Dictionary<Type, List<ConstantReaction>>> GroupReactions(IEnumerable<ConstantReaction> handlers, IEventSerializer serializer)
    {
        Dictionary<string, Dictionary<Type, List<ConstantReaction>>> identifiedReactions = new();

        foreach (ConstantReaction handler in handlers)
        {
            string identifier = serializer.GetIdentifier(handler.TargetType);

            if (!identifiedReactions.TryGetValue(identifier, out var typeReactions))
            {
                typeReactions = new Dictionary<Type, List<ConstantReaction>>();
                identifiedReactions.Add(identifier, typeReactions);
            }

            if (!typeReactions.TryGetValue(handler.TargetType, out var identifierReactions))
            {
                identifierReactions = new List<ConstantReaction>();
                typeReactions.Add(handler.TargetType, identifierReactions);
            }

            identifierReactions.Add(handler);
        }

        return identifiedReactions;
    }

    private static string GetStreamName(string reactionKey) => $"$reactions-{reactionKey}";
    private static async Task<StreamPosition> GetReactionPosition(string reactionKey, IServiceProvider services)
    {
        IEventSerializer _eventSerializer = services.GetRequiredService<IEventSerializer>();
        IEventStoreLite _eventStreamConnection = services.GetRequiredService<IEventStoreLite>();
        string reactionEventIdentifier = _eventSerializer.GetIdentifier(typeof(ReactionHandled));

        await foreach (StreamEvent streamEvent in _eventStreamConnection.ReadStreamEvents(GetStreamName(reactionKey), StreamDirection.Reverse, StreamPosition.End))
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
    private static Task StoreReactionPosition(string reactionKey, StreamEvent streamEvent, IServiceProvider services)
    {
        IStreamEventWriter _streamEventWriter = services.GetRequiredService<IStreamEventWriter>();

        ReactionHandled handledEvent = new()
        {
            GlobalOrdinal = streamEvent.GlobalOrdinal,
        };

        return _streamEventWriter.AppendToStream(GetStreamName(reactionKey), handledEvent);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationTokenSource?.Cancel();
        await _completionTask;
    }
}
