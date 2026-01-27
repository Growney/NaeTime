using NaeTime.Bytes;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.Collections.Concurrent;

namespace NaeTime.Hardware.Node.Esp32;

public class NodeConfigurationProtocol(INodeCommunication nodeCommunication) : INodeConfigurationProtocol
{
    private const int _timeoutInSeconds = 30;

    private readonly INodeCommunication _nodeCommunication = nodeCommunication ?? throw new ArgumentNullException(nameof(nodeCommunication));
    private ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<bool>>>> _commandLaneAckWaiting = new();
    private ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<bool>>> _commandAckWaiting = new();
    private ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<byte[]>>>> _commandLaneDataWaiting = new();
    private ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<byte[]>>> _commandDataWaiting = new();

    public ValueTask<bool> SetLaneFrequency(byte lane,byte? band, ushort frequencyInMHz, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(NodeProtocol.START_OF_RECORD);

        writer.WriteRecordType(RecordType.TUNE_LANE);
        writer.Write(lane);
        writer.Write(band ?? 0);
        writer.Write(frequencyInMHz);

        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        return SendWait<bool>(RecordType.TUNE_LANE, lane, finalisedData, token, _commandLaneAckWaiting);
    }

    private ValueTask<bool> SetNodeThreshold(RecordType commandId, byte lane, ushort threshold, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(NodeProtocol.START_OF_RECORD);

        writer.WriteRecordType(commandId);
        writer.Write(lane);
        writer.Write(threshold);

        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        return SendWait<bool>(commandId, lane, finalisedData, token, _commandLaneAckWaiting);
    }
    public ValueTask<bool> SetEntryThreshold(byte lane, ushort threshold, CancellationToken token = default)
       => SetNodeThreshold(RecordType.CONFIGURE_LANE_ENTRY_THRESHOLD, lane, threshold, token);
    public ValueTask<bool> SetExitThreshold(byte lane, ushort threshold, CancellationToken token = default)
        => SetNodeThreshold(RecordType.CONFIGURE_LANE_EXIT_THRESHOLD, lane, threshold, token);

    public ValueTask<bool> SetLaneEnabled(byte lane, bool isEnabled, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(NodeProtocol.START_OF_RECORD);

        writer.WriteRecordType(RecordType.CONFIGURE_LANE_ENABLED);
        writer.Write(lane);
        writer.Write((byte)(isEnabled ? 1 : 0));

        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        return SendWait<bool>(RecordType.CONFIGURE_LANE_ENABLED, lane, finalisedData, token, _commandLaneAckWaiting);
    }

    public async ValueTask<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfiguration(IEnumerable<byte> laneIds)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(NodeProtocol.START_OF_RECORD);
        writer.WriteRecordType(RecordType.REQUEST_LANE_CONFIGURATIONS);

        byte lanes = 0;
        for(int i = 0; i < 8; i++)
        {
            if (laneIds.Contains((byte)i))
            {
                lanes |= (byte)(1 << i);
            }
        }
        writer.Write(lanes);
        writer.Write(NodeProtocol.END_OF_RECORD);
        byte[] finalisedData = memoryStream.FinalisePacketData();

        byte[] responseData = await SendWait<byte[]>(RecordType.REQUEST_LANE_CONFIGURATIONS, finalisedData, CancellationToken.None, Array.Empty<byte>(), _commandDataWaiting);

        List<NaeTimeNodeLaneConfiguration> configurations = new();

        ReadOnlySpanReader<byte> responseReader = new(responseData);

        byte enabledLanes = responseReader.ReadByte();

        byte laneId = 0;
        while (responseReader.HasData())
        {
            byte bandId = responseReader.ReadByte();
            ushort frequencyInMHz = responseReader.ReadUInt16();
            ushort entryThreshold = responseReader.ReadUInt16();
            ushort exitThreshold = responseReader.ReadUInt16();
            bool isEnabled = (enabledLanes & (1 << laneId)) != 0;
            configurations.Add(new NaeTimeNodeLaneConfiguration(laneId, bandId, frequencyInMHz,isEnabled, entryThreshold, exitThreshold));
            laneId++;
        }

        return configurations;
    }

    private async ValueTask<T> SendWait<T>(RecordType command, byte lane, byte[] finalisedData, CancellationToken token,ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>>> sources)
    {
        ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>> commandWaiting = sources.GetOrAdd((byte)command, x => new ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>>());

        ConcurrentQueue<TaskCompletionSource<T>> laneQueue = commandWaiting.GetOrAdd(lane, x => new ConcurrentQueue<TaskCompletionSource<T>>());

        TaskCompletionSource<T> tuneAck = new(TaskCreationOptions.RunContinuationsAsynchronously);
        laneQueue.Enqueue(tuneAck);
        TimeOutToken(TimeSpan.FromSeconds(_timeoutInSeconds), tuneAck);
        await _nodeCommunication.SendAsync(finalisedData, token);
        T result = await tuneAck.Task;
        return result;
    }
    private async ValueTask<T> SendWait<T>(RecordType command, byte[] finalisedData, CancellationToken token, T timeOut, ConcurrentDictionary<byte,ConcurrentQueue<TaskCompletionSource<T>>> sources)
    {
        ConcurrentQueue<TaskCompletionSource<T>> commandQueue = sources.GetOrAdd((byte)command, x => new ConcurrentQueue<TaskCompletionSource<T>>());

        TaskCompletionSource<T> tuneAck = new(TaskCreationOptions.RunContinuationsAsynchronously);
        commandQueue.Enqueue(tuneAck);
        TimeOutToken(TimeSpan.FromSeconds(_timeoutInSeconds), tuneAck);
        await _nodeCommunication.SendAsync(finalisedData, token);
        T result = await tuneAck.Task;
        return result;
    }

    private static void TimeOutToken<T>(TimeSpan timeOut, TaskCompletionSource<T> taskCompletionSource) => Task.Delay(timeOut).ContinueWith(t => taskCompletionSource.TrySetCanceled());
    public void HandleRecordData(RecordType commandId, ReadOnlySpanReader<byte> recordReader)
    {

    }

    private static bool IsSingleLaneCommand(RecordType recordType)
    {
        return recordType switch
        {
            RecordType.TUNE_LANE
            or RecordType.CONFIGURE_LANE_ENTRY_THRESHOLD
            or RecordType.CONFIGURE_LANE_EXIT_THRESHOLD
            or RecordType.CONFIGURE_LANE_ENABLED => true,
            _ => false
        };
    }

    private static void HandleResponse<T>(RecordType commandId, ReadOnlySpanReader<byte> ackReader, T setResult, ConcurrentDictionary<byte, ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>>> laneWaiting, ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>> commandWaiting)
    {
        if (IsSingleLaneCommand(commandId))
        {

            if (!laneWaiting.TryGetValue((byte)commandId, out ConcurrentDictionary<byte, ConcurrentQueue<TaskCompletionSource<T>>>? laneAcks))
            {
                return;
            }

            byte laneId = ackReader.ReadByte();

            if (laneAcks.TryGetValue(laneId, out ConcurrentQueue<TaskCompletionSource<T>>? taskCompletionQueue))
            {
                while (taskCompletionQueue.TryDequeue(out TaskCompletionSource<T>? dequeuedAck))
                {
                    dequeuedAck.TrySetResult(setResult);
                    return;
                }
            }
            return;
        }
        else if (commandWaiting.TryRemove((byte)commandId, out ConcurrentQueue<TaskCompletionSource<T>>? taskCompletionQueue))
        {
            while (taskCompletionQueue.TryDequeue(out TaskCompletionSource<T>? dequeuedAck))
            {
                dequeuedAck.TrySetResult(setResult);
                return;
            }
            return;
        }
    }
    public void HandleResponseData(RecordType response, RecordType commandId, ReadOnlySpanReader<byte> recordReader)
    {
        if (response == RecordType.ACK)
        {
            HandleResponse(commandId, recordReader, true, _commandLaneAckWaiting, _commandAckWaiting);
        }
        else if (response == RecordType.ERROR)
        {
            HandleResponse(commandId, recordReader, false, _commandLaneAckWaiting, _commandAckWaiting);
        }
        else if (response == RecordType.RESPONSE)
        {
            byte[] data = recordReader.ReadRemaining();
            HandleResponse(commandId, recordReader, data, _commandLaneDataWaiting, _commandDataWaiting);
        }
    }

}
