using EventDbLite.Reactions;
using System.Runtime.CompilerServices;

namespace EventDbLite.Abstractions;
public static class IReactionProviderExtensions
{
    public static IDisposable On(this IReactionProvider reactionProvider, Type type, Func<object, Task> handler) => reactionProvider.On(type, (obj) => handler(obj.Payload));
    public static IDisposable On<T>(this IReactionProvider reactionProvider, Func<T, Task> handler)
    {
        if (reactionProvider is null)
        {
            throw new ArgumentNullException(nameof(reactionProvider));
        }
        if (handler is null)
        {
            throw new ArgumentNullException(nameof(handler));
        }
        return reactionProvider.On(typeof(T), async (reactionEvent) =>
        {
            if (reactionEvent.Payload is T t)
            {
                await handler(t).ConfigureAwait(false);
            }
        });
    }

    public static async IAsyncEnumerable<T> StreamEvents<T>(this IReactionProvider reactionProvider, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (reactionProvider is null)
        {
            throw new ArgumentNullException(nameof(reactionProvider));
        }

        await foreach (ReactionEvent obj in reactionProvider.StreamSubscription(typeof(T), cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (obj.Payload is T t)
            {
                yield return t;
            }
        }
    }
}
