using EventDbLite.Abstractions;
using System.Runtime.CompilerServices;

namespace EventDbLite.Extensions;
public static class IReactionProviderExtensions
{
    public static IDisposable On(this IReactionProvider reactionProvider, Type type, Func<object, Task> handler) => reactionProvider.On(type, (obj, _) => handler(obj));
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
        return reactionProvider.On(typeof(T), async (obj) =>
        {
            if (obj is T t)
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

        await foreach (object obj in reactionProvider.StreamEvents(typeof(T), cancellationToken).WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            if (obj is T t)
            {
                yield return t;
            }
        }
    }
}
