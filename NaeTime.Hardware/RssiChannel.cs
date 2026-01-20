using NaeTime.Collections;
using NaeTime.Hardware.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace NaeTime.Hardware;

internal class RssiChannel : IRssiChannel
{
    public AwaitableQueue<RssiValue> ChannelQueue { get; } = new(0);

    public void AppendRssi(Guid timerId, byte lane, float rssi, long softwareTime, ulong? hardwareTime)
    {
        ChannelQueue.Enqueue(new RssiValue(timerId, lane, rssi, softwareTime, hardwareTime));
    }
}
