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
        if (_queue.Count > _maxSize && _maxSize > 0)
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
            catch (OperationCanceledException)
            {
                return default;
            }
        }

        return _queue.TryDequeue(out T? item) ? item : default;
    }

    public async Task<T[]> WaitForDequeueAsync(int count, CancellationToken cancellationToken = default)
    {
        T[] items = new T[count];
        int insertIndex = 0;
        while (insertIndex < count && !cancellationToken.IsCancellationRequested)
        {
            T? item = await WaitForDequeueAsync(cancellationToken);
            if (item != null)
            {
                items[insertIndex] = item;
                insertIndex++;
            }
        }

        return items.ToArray();
    }
}
