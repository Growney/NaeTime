namespace EventDbLite.Abstractions;

public interface IReactionProvider
{
    IDisposable On<T>(Func<T, Task> handler);
    IAsyncEnumerable<T> GetEvents<T>(CancellationToken cancellationToken);
}
