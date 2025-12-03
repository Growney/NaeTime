using EventDbLite.Abstractions;
using EventDbLite.Projections;
using EventDbLite.Reactions.Abstractions;
using EventDbLite.Streams;

namespace EventDbLite.Reactions;
public class ReactionProviderFactory : IReactionProviderFactory
{
    private readonly IEventStoreLite _eventStore;
    private readonly IEventSerializer _eventSerializer;
    private readonly IEnumerable<LiveProjectionRequirement> _requirements;
    private readonly ILiveProjectionRepository _repository;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public ReactionProviderFactory(IEventStoreLite eventStore, IEventSerializer eventSerializer,IEnumerable<LiveProjectionRequirement> requirements, ILiveProjectionRepository repository)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IReactionProvider<TEvent> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
        => CreateProvider<TEvent>(initialPosition, _requirements.Select(r => r.ProjectionType), streamName);
    public IReactionProvider<TEvent> CreateProvider<TEvent>(StreamPosition initialPosition,IEnumerable<Type> requirements, string? streamName = null)
        => new ReactionProvider<TEvent>(_eventStore, _eventSerializer,requirements,_repository, initialPosition, streamName, _cancellationTokenSource.Token);

    public void Dispose() => _cancellationTokenSource.Cancel();
}
