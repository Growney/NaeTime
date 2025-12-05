using EventDbLite.Abstractions;
using EventDbLite.Projections;
using EventDbLite.Reactions.Abstractions;

namespace EventDbLite.Reactions;
public class ReactionProviderFactory : IReactionProviderFactory
{
    private readonly IEventStoreLite _eventStore;
    private readonly IEventSerializer _eventSerializer;
    private readonly IEnumerable<LiveProjectionRequirement> _requirements;
    private readonly ILiveProjectionRepository _repository;

    public ReactionProviderFactory(IEventStoreLite eventStore, IEventSerializer eventSerializer, IEnumerable<LiveProjectionRequirement> requirements, ILiveProjectionRepository repository)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
        => CreateProvider<TEvent>(initialPosition, _requirements.Select(r => r.ProjectionType), streamName);
    private IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, IEnumerable<Type> requirements, string? streamName = null)
        => new ReactionProvider<TEvent>(_eventStore, _eventSerializer, requirements, _repository, initialPosition, streamName);

}
