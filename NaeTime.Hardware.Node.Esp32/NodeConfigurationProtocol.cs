using NaeTime.Bytes;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;
public class NodeConfigurationProtocol : INodeConfigurationProtocol
{
    private readonly INodeCommunication _nodeCommunication;
    private ConcurrentDictionary<byte, ConcurrentDictionary<byte, TaskCompletionSource<bool>>> _laneAckWaiting = new();

    public NodeConfigurationProtocol(INodeCommunication nodeCommunication)
    {
        _nodeCommunication = nodeCommunication ?? throw new ArgumentNullException(nameof(nodeCommunication));
    }
    private byte GetNodeLaneId(byte lane) => (byte)(lane - 1);
    public async ValueTask SetLaneFrequency(byte lane, ushort frequencyInMHz, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        byte nodeLane = GetNodeLaneId(lane);

        writer.Write(NodeProtocol.START_OF_RECORD);

        writer.WriteRecordType(RecordType.TUNE_LANE);
        writer.Write(nodeLane);
        writer.Write(frequencyInMHz);

        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        while (!await SendWithWaitForAck(RecordType.TUNE_LANE, nodeLane, finalisedData, token))
        {
            await Task.Delay(500);
        }
    }
    private async ValueTask<bool> SendWithWaitForAck(RecordType command, byte lane, byte[] finalisedData, CancellationToken token)
        => await SendWithWaitForAck((byte)command, lane, finalisedData, token);
    private async ValueTask<bool> SendWithWaitForAck(byte command, byte lane, byte[] finalisedData, CancellationToken token)
    {
        ConcurrentDictionary<byte, TaskCompletionSource<bool>> laneAckWaiting = _laneAckWaiting.GetOrAdd(command, x => new ConcurrentDictionary<byte, TaskCompletionSource<bool>>());

        TaskCompletionSource<bool> tuneAck = laneAckWaiting.AddOrUpdate(lane, x => new TaskCompletionSource<bool>(), (x, t) =>
        {
            t.TrySetCanceled();
            return new TaskCompletionSource<bool>();
        });
        TimeOutToken(TimeSpan.FromSeconds(5), tuneAck);
        await _nodeCommunication.SendAsync(finalisedData, token);
        bool result = await tuneAck.Task;
        return result;

    }
    private void TimeOutToken(TimeSpan timeOut, TaskCompletionSource<bool> taskCompletionSource) => Task.Delay(timeOut).ContinueWith(t => taskCompletionSource.TrySetResult(false));
    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {

    }
    private void HandleResponse(ReadOnlySpanReader<byte> ackReader, bool setResult)
    {
        byte recordType = ackReader.ReadByte();

        if (!_laneAckWaiting.TryGetValue(recordType, out ConcurrentDictionary<byte, TaskCompletionSource<bool>>? laneAcks))
        {
            return;
        }

        byte laneId = (RecordType)recordType switch
        {
            RecordType.TUNE_LANE
            or RecordType.CONFIGURE_LANE_ENTRY_THRESHOLD
            or RecordType.CONFIGURE_LANE_EXIT_THRESHOLD => ackReader.ReadByte(),
            _ => throw new NotImplementedException()
        };

        if (laneAcks.TryRemove(laneId, out TaskCompletionSource<bool>? taskCompletionSource))
        {
            taskCompletionSource.TrySetResult(setResult);
        }
    }
    public void HandleResponseData(byte response, ReadOnlySpanReader<byte> recordReader) => HandleResponse(recordReader, response == (byte)RecordType.ACK);

    private async ValueTask SetNodeThreshold(RecordType commandId, byte lane, ushort threshold, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        byte nodeLane = GetNodeLaneId(lane);

        writer.Write(NodeProtocol.START_OF_RECORD);

        writer.WriteRecordType(commandId);
        writer.Write(nodeLane);
        writer.Write(threshold);

        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        while (!await SendWithWaitForAck(commandId, nodeLane, finalisedData, token))
        {
            await Task.Delay(1000);
        }
    }
    public ValueTask SetEntryThreshold(byte lane, ushort threshold, CancellationToken token = default)
        => SetNodeThreshold(RecordType.CONFIGURE_LANE_ENTRY_THRESHOLD, lane, threshold, token);
    public ValueTask SetExitThreshold(byte lane, ushort threshold, CancellationToken token = default)
        => SetNodeThreshold(RecordType.CONFIGURE_LANE_EXIT_THRESHOLD, lane, threshold, token);
}
