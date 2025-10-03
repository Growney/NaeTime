using EventDbLite.Abstractions;
using EventDbLite.Events;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EventDbLite.Reactions;
public class ReactionService : IHostedService
{
    private const string ReactionStreamName = "$reactions";
    private readonly IServiceProvider _serviceProvider;

    private IReactionProvider? _reactionProvider;

    public ReactionService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        IReactionProviderFactory factory = _serviceProvider.GetRequiredService<IReactionProviderFactory>();

        StreamPosition position = await GetReactionPosition();

        IReactionProvider provider = factory.CreateProvider(position);

        IEnumerable<ConstantReactionSource> reactionSources = _serviceProvider.GetServices<ConstantReactionSource>();

        foreach (ConstantReactionSource reactionSource in reactionSources)
        {
            foreach (ConstantReaction reaction in reactionSource.Reactions)
            {
                provider.On(reaction.TargetType, (eventObj, metadata) => ReactToEvent(reaction, eventObj, metadata));
            }
        }
    }
    private async Task<StreamPosition> GetReactionPosition()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();
        IEventSerializer _eventSerializer = scope.ServiceProvider.GetRequiredService<IEventSerializer>();
        IEventStoreLite _eventStreamConnection = scope.ServiceProvider.GetRequiredService<IEventStoreLite>();
        string reactionEventIdentifier = _eventSerializer.GetIdentifier(typeof(ReactionHandled));

        await foreach (StreamEvent streamEvent in _eventStreamConnection.ReadStreamEvents(ReactionStreamName, StreamDirection.Reverse, StreamPosition.End))
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
    private async Task ReactToEvent(ConstantReaction reaction, object obj, StreamEvent streamEvent)
    {
        IServiceScope? scope = null;
        try
        {
            scope = _serviceProvider.CreateScope();
            await reaction.Handler(scope.ServiceProvider, obj);

            IStreamEventWriter _streamEventWriter = scope.ServiceProvider.GetRequiredService<IStreamEventWriter>();

            ReactionHandled handledEvent = new()
            {
                GlobalOrdinal = streamEvent.GlobalOrdinal,
            };

            await _streamEventWriter.AppendToStream(ReactionStreamName, handledEvent);
        }
        finally
        {
            scope?.Dispose();
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_reactionProvider != null)
        {
            await _reactionProvider.DisposeAsync();
        }
    }
}
