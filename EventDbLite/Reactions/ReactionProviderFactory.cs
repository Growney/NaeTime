using EventDbLite.Abstractions;
using EventDbLite.Streams;

namespace EventDbLite.Reactions;
public class ReactionProviderFactory : IReactionProviderFactory
{
    private readonly IEventStoreLite _eventStore;
    private readonly IEventSerializer _eventSerializer;
    public ReactionProviderFactory(IEventStoreLite eventStore, IEventSerializer eventSerializer)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    public IReactionProvider CreateProvider(StreamPosition initialPosition, string? streamName = null)
    {
        IStreamSubscription subscription = streamName is not null
            ? _eventStore.SubscribeToStream(streamName, initialPosition)
            : _eventStore.SubscribeToAllStreams(initialPosition);

        return new ReactionProvider(subscription, _eventSerializer);

    }
}
