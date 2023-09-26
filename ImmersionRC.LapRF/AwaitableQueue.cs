using System.Collections.Concurrent;

namespace ImmersionRC.LapRF;
public class AwaitableQueue<T> : IDisposable
{
    private readonly ConcurrentQueue<T> _queue = new ConcurrentQueue<T>();
    private readonly SemaphoreSlim _signal = new SemaphoreSlim(0);
    private readonly int _maxSize;
    public AwaitableQueue(int maxSize)
    {
        _maxSize = maxSize;
    }

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
            await _signal.WaitAsync(cancellationToken);
        }

        return _queue.TryDequeue(out var item) ? item : default;
    }
}
