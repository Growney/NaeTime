using NaeTime.Collections;
using NaeTime.Hardware.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NaeTime.Hardware;
public class RssiChannel : IRssiChannel
{
    private ConcurrentDictionary<Guid, AwaitableQueue<RssiValue>> _rssiValues = new();
    public async IAsyncEnumerator<RssiValue> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        Guid consumerId = Guid.NewGuid();

        AwaitableQueue<RssiValue> rssiQueue = new(100);

        _rssiValues.TryAdd(consumerId, rssiQueue);

        while(!cancellationToken.IsCancellationRequested)
        {
            RssiValue? rssiValue = await rssiQueue.WaitForDequeueAsync(cancellationToken).ConfigureAwait(false);
            if (rssiValue != null)
            {
                yield return rssiValue;
            }
        }

        _rssiValues.TryRemove(consumerId, out _);
    }

    public ValueTask WriteAsync(RssiValue value)
    {
        foreach(var kvp in _rssiValues)
        {
            kvp.Value.Enqueue(value);
        }
        return ValueTask.CompletedTask;
    }
}
