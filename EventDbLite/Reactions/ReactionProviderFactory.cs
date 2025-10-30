using EventDbLite.Abstractions;
using EventDbLite.Streams;

namespace EventDbLite.Reactions;
public class ReactionProviderFactory : IReactionProviderFactory
{
    private readonly IEventStoreLite _eventStore;
    private readonly IEventSerializer _eventSerializer;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ReactionProviderFactory(IEventStoreLite eventStore, IEventSerializer eventSerializer)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
    }

    public IReactionProvider<TEvent> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
        => new ReactionProvider<TEvent>(_eventStore, _eventSerializer, initialPosition, streamName, _cancellationTokenSource.Token);

    public void Dispose() => _cancellationTokenSource.Cancel();
}
