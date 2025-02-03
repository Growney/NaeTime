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
        ulong currentTime = recordReader.ReadUInt32();
        byte laneCount = recordReader.ReadByte();
        byte enabledLanes = recordReader.ReadByte();

        for (byte lane = 0; lane < laneCount; lane++)
        {
            if ((enabledLanes & (1 << lane)) == 0)
            {
                continue;
            }

            byte translatedLane = (byte)(lane + 1);
            ushort rssi = recordReader.ReadUInt16();
            ulong lastPass = recordReader.ReadUInt32();
            ushort passCount = recordReader.ReadUInt16();

            ushort? lastRecordPassCount = _lanePassStats.GetOrAdd(lane, (ushort?)null);

            if (lastRecordPassCount != null && passCount != 0 && passCount != lastRecordPassCount)
            {
                _passQueue.Enqueue(new Pass(translatedLane, lastPass));
            }

            _lanePassStats.AddOrUpdate(lane, passCount, (x, t) => passCount);

            _receivedSignalStrengthIndicators.Enqueue(new ReceivedSignalStrengthIndicator(translatedLane, rssi, currentTime));
        }
    }

    public void HandleResponseData(byte response, ReadOnlySpanReader<byte> recordReader)
    {

    }
}
