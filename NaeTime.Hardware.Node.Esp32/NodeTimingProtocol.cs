using NaeTime.Bytes;
using NaeTime.Collections;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeTimingProtocol : INodeTimingProtocol
{
    private readonly AwaitableQueue<ReceivedSignalStrengthIndicator?> _receivedSignalStrengthIndicators = new(5000);
    private readonly ConcurrentDictionary<byte, ReceivedSignalStrengthIndicator> _lastReceivedSignalStrengthIndicators = new();

    private readonly ConcurrentDictionary<byte, (long, long, int)> _lanePassStats = new();
    private void AddReceivedSignalStrengthIndicator(ReceivedSignalStrengthIndicator receivedSignalStrengthIndicator)
    {
        _lastReceivedSignalStrengthIndicators.AddOrUpdate(receivedSignalStrengthIndicator.Lane
            , receivedSignalStrengthIndicator, (key, oldValue) => receivedSignalStrengthIndicator);
        _receivedSignalStrengthIndicators.Enqueue(receivedSignalStrengthIndicator);
    }

    public Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken) => _receivedSignalStrengthIndicators.WaitForDequeueAsync(cancellationToken);

    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte lane = (byte)(recordReader.ReadByte() + 1);
        ulong time = (ulong)recordReader.ReadInt32();
        ushort rssi = recordReader.ReadUInt16();
        long lastPassStart = recordReader.ReadInt32();
        long lastPassEnd = recordReader.ReadInt32();
        short passState = recordReader.ReadInt16();
        int passCount = recordReader.ReadInt32();

        AddReceivedSignalStrengthIndicator(new ReceivedSignalStrengthIndicator(lane, rssi, time));
    }
}
