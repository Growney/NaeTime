using NaeTime.Bytes;
using NaeTime.Collections;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeTimingProtocol : INodeTimingProtocol
{
    private readonly AwaitableQueue<ReceivedSignalStrengthIndicator?> _receivedSignalStrengthIndicators = new(5000);
    private readonly AwaitableQueue<Pass> _passQueue = new(5000);

    public Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken) => _receivedSignalStrengthIndicators.WaitForDequeueAsync(cancellationToken);
    public Task<Pass> WaitForNextPassAsync(CancellationToken cancellationToken) => _passQueue.WaitForDequeueAsync(cancellationToken);
    public void HandleRecordData(RecordType commandId,ReadOnlySpanReader<byte> recordReader)
    {
        switch(commandId)
        {
            case RecordType.LANE_PASS_EVENT:
                HandlePassEvent(recordReader);
                break;
            case RecordType.NODE_STATUS:
                HandleStatus(recordReader);
                break;
        }
    }

    private void HandlePassEvent(ReadOnlySpanReader<byte> recordReader)
    {
        byte lane = recordReader.ReadByte();
        ushort passCount = recordReader.ReadUInt16();
        ulong passStart = recordReader.ReadUInt64();
        ulong passEnd = recordReader.ReadUInt64();

        ulong passMiddle = passStart + ((passEnd - passStart) / 2);

        _passQueue.Enqueue(new Pass(lane, passMiddle));
    }

    private void HandleStatus(ReadOnlySpanReader<byte> recordReader)
    {
        ulong currentTime = recordReader.ReadUInt64();
        byte laneCount = recordReader.ReadByte();
        byte enabledLanes = recordReader.ReadByte();

        for (byte lane = 0; lane < laneCount; lane++)
        {
            if ((enabledLanes & (1 << lane)) == 0)
            {
                continue;
            }
            byte currentLane = recordReader.ReadByte();
            ulong rssiReadTime = recordReader.ReadUInt64();
            ushort rssi = recordReader.ReadUInt16();

            _receivedSignalStrengthIndicators.Enqueue(new ReceivedSignalStrengthIndicator(currentLane, rssi, rssiReadTime));
        }
    }

    public void HandleResponseData(RecordType responseCode, RecordType commandId, ReadOnlySpanReader<byte> recordReader)
    {

    }
}
