using System.Collections.Concurrent;

namespace NaeTime.Collections;
public class AwaitableQueue<T>(int maxSize) : IDisposable
{
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly SemaphoreSlim _signal = new(0);
    private readonly int _maxSize = maxSize;

    public void Dispose()
    {
        _signal.Dispose();
    }

    public void Enqueue(T item)
    {
        _queue.Enqueue(item);
        if (_queue.Count > _maxSize)
        {
            _queue.TryDequeue(out _);
        }

        _signal.Release();
    }

    public async Task<T?> WaitForDequeueAsync(CancellationToken cancellationToken = default)
    {
        if (_queue.IsEmpty)
        {
            try
            {
                await _signal.WaitAsync(cancellationToken);
            }
            catch(OperationCanceledException)
            {
                return default;
            }
        }

        return _queue.TryDequeue(out T? item) ? item : default;
    }
}
