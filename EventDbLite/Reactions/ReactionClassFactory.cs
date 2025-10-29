using EventDbLite.Abstractions;
using EventDbLite.Streams;
using Microsoft.Extensions.DependencyInjection;

namespace EventDbLite.Reactions;
public class ReactionClassFactory : IReactionClassFactory
{
    private readonly IReactionProviderFactory _reactionProviderFactory;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAsyncHandlerProvider _handlerProvider;

    public ReactionClassFactory(IReactionProviderFactory reactionProviderFactory, IServiceProvider serviceProvider, IAsyncHandlerProvider handlerProvider)
    {
        _reactionProviderFactory = reactionProviderFactory ?? throw new ArgumentNullException(nameof(reactionProviderFactory));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _handlerProvider = handlerProvider ?? throw new ArgumentNullException(nameof(handlerProvider));
    }

    public ReactionClassContainer<T> Create<T>()
    {
        T instance = ActivatorUtilities.CreateInstance<T>(_serviceProvider);
        IReactionProvider reactionProvider = _reactionProviderFactory.CreateProvider(StreamPosition.Beginning);
        return new ReactionClassContainer<T>(instance, reactionProvider, _handlerProvider);
    }
}
