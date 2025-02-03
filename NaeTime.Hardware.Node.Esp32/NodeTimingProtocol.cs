using NaeTime.Bytes;
using NaeTime.Collections;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeTimingProtocol : INodeTimingProtocol
{
    private readonly AwaitableQueue<ReceivedSignalStrengthIndicator?> _receivedSignalStrengthIndicators = new(5000);
    private readonly AwaitableQueue<Pass> _passQueue = new(5000);

    private readonly ConcurrentDictionary<byte, ushort?> _lanePassStats = new();

    public Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken) => _receivedSignalStrengthIndicators.WaitForDequeueAsync(cancellationToken);
    public Task<Pass> WaitForNextPassAsync(CancellationToken cancellationToken) => _passQueue.WaitForDequeueAsync(cancellationToken);
    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte lane = (byte)(recordReader.ReadByte() + 1);
        ulong time = (ulong)recordReader.ReadInt32();
        ushort rssi = recordReader.ReadUInt16();
        ulong lastPassStart = (ulong)recordReader.ReadInt32();
        ulong lastPassEnd = (ulong)recordReader.ReadInt32();
        byte passState = recordReader.ReadByte();
        ushort passCount = recordReader.ReadUInt16();

        ushort? lastRecordPassCount = _lanePassStats.GetOrAdd(lane, (ushort?)null);

        if (lastRecordPassCount != null && passCount != 0 && passCount != lastRecordPassCount)
        {
            _passQueue.Enqueue(new Pass(lane, lastPassStart, lastPassEnd, time));
        }

        _lanePassStats.AddOrUpdate(lane, passCount, (x, t) => passCount);

        _receivedSignalStrengthIndicators.Enqueue(new ReceivedSignalStrengthIndicator(lane, rssi, time));
    }

    public void HandleResponseData(byte response, ReadOnlySpanReader<byte> recordReader)
    {

    }
}
