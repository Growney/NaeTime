using EventDbLite.Abstractions;
using EventDbLite.Handlers;

namespace EventDbLite.Reactions;
public class ReactionClassContainer<T> : IAsyncDisposable
{
    private readonly IReactionProvider _reactionProvider;
    public T Instance { get; }
    public ReactionClassContainer(T instance, IReactionProvider reactionProvider, IAsyncHandlerProvider handlerProvider)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _reactionProvider = reactionProvider ?? throw new ArgumentNullException(nameof(reactionProvider));

        IEnumerable<AsyncHandler> handlers = handlerProvider.GetHandlerMethods(typeof(T));

        foreach (AsyncHandler handler in handlers)
        {
            _reactionProvider.On(handler.TargetType, (reactionEvent) => handler.Action.Invoke(Instance, reactionEvent.Payload));
        }
    }

    public ValueTask DisposeAsync() => _reactionProvider.DisposeAsync();
}
