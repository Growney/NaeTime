using EventDbLite.Abstractions;
using EventDbLite.Projections;
using EventDbLite.Reactions.Abstractions;
using Microsoft.Extensions.Logging;

namespace EventDbLite.Reactions;
public class ReactionProviderFactory : IReactionProviderFactory
{
    private readonly IEventStoreLite _eventStore;
    private readonly IEventSerializer _eventSerializer;
    private readonly IEnumerable<LiveProjectionRequirement> _requirements;
    private readonly ILiveProjectionRepository _repository;
    private readonly ILoggerFactory _loggerProvider;

    public ReactionProviderFactory(IEventStoreLite eventStore, IEventSerializer eventSerializer, IEnumerable<LiveProjectionRequirement> requirements, ILiveProjectionRepository repository, ILoggerFactory loggerProvider)
    {
        _eventStore = eventStore ?? throw new ArgumentNullException(nameof(eventStore));
        _eventSerializer = eventSerializer ?? throw new ArgumentNullException(nameof(eventSerializer));
        _requirements = requirements ?? throw new ArgumentNullException(nameof(requirements));
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _loggerProvider = loggerProvider ?? throw new ArgumentNullException(nameof(loggerProvider));
    }

    public IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, string? streamName = null)
        => CreateProvider<TEvent>(initialPosition, _requirements.Select(r => r.ProjectionType), streamName);
    private IAsyncEnumerable<ReactionEvent<TEvent>> CreateProvider<TEvent>(StreamPosition initialPosition, IEnumerable<Type> requirements, string? streamName = null)
    {
        ILogger<ReactionProvider<TEvent>> logger = _loggerProvider.CreateLogger<ReactionProvider<TEvent>>();    
        return new ReactionProvider<TEvent>(_eventStore, _eventSerializer, requirements, _repository, initialPosition,logger, streamName);
    }
}
