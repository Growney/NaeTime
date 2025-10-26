using ImmersionRC.LapRF.Abstractions;
using NaeTime.Bytes;
using System.Collections.Concurrent;

namespace ImmersionRC.LapRF.Protocol;
internal class RadioFrequencySetupProtocol(ILapRFCommunication lapRFCommunication) : IRadioFrequencySetupProtocol
{
    private readonly ILapRFCommunication _lapRFCommunication = lapRFCommunication;
    private readonly ConcurrentDictionary<byte, RFSetup> _latestSetup = new();
    private readonly ConcurrentDictionary<byte, List<TaskCompletionSource<RFSetup>>> _responders = new();
    private readonly Crc16 _crc16 = new();

    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte? transponderId = null;
        ushort? isEnabled = null;
        ushort? channel = null;
        ushort? band = null;
        ushort? attenuation = null;
        ushort? frequency = null;
        float? threshold = null;

        while (recordReader.HasData())
        {
            byte fieldSignature = recordReader.ReadByte();

            if (fieldSignature == LapRFProtocol.END_OF_RECORD)
            {
                break;
            }
            ///read this to clear off the buffer but i am not going to check it because i don't think it makes a difference if it does then i will have to change this
            _ = recordReader.ReadByte();

            switch ((RadioFrequencySetupField)fieldSignature)
            {
                case RadioFrequencySetupField.TransponderId:
                    transponderId = recordReader.ReadByte();
                    break;
                case RadioFrequencySetupField.IsEnabled:
                    isEnabled = recordReader.ReadUInt16();
                    break;
                case RadioFrequencySetupField.Channel:
                    channel = recordReader.ReadUInt16();
                    break;
                case RadioFrequencySetupField.Band:
                    band = recordReader.ReadUInt16();
                    break;
                case RadioFrequencySetupField.Attenuation:
                    attenuation = recordReader.ReadUInt16();
                    break;
                case RadioFrequencySetupField.Frequency:
                    frequency = recordReader.ReadUInt16();
                    break;
                case RadioFrequencySetupField.Threshold:
                    threshold = recordReader.ReadFloat();
                    break;
                default:
                    break;
            }
        }

        if (transponderId.HasValue)
        {
            RFSetup setup = new(LapRFProtocol.GetLaneId(transponderId.Value), isEnabled == 1, channel, band, attenuation, frequency, threshold);
            HandleTransponderSetup(setup);
        }
    }
    private void HandleTransponderSetup(RFSetup setup)
    {
        _latestSetup.AddOrUpdate(setup.LaneId, setup,
                    (key, oldValue) => setup);

        if (_responders.TryGetValue(setup.LaneId, out List<TaskCompletionSource<RFSetup>>? responders))
        {
            foreach (TaskCompletionSource<RFSetup> responder in responders)
            {
                responder.SetResult(setup);
            }

            responders.Clear();
        }
    }
    public ValueTask SetupLane(byte laneId, bool? isEnabled, ushort? channel = null, ushort? band = null, ushort? attenuation = null, ushort? frequencyInMHz = null, float? threshold = null, CancellationToken token = default)
    {
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(LapRFProtocol.START_OF_RECORD);
        writer.WriteRecordType(RecordType.LAPRF_TOR_RFSETUP);
        writer.WriteField((byte)RadioFrequencySetupField.TransponderId, LapRFProtocol.GetTransponderId(laneId));
        if (isEnabled.HasValue)
        {
            ushort enabledFlag = (ushort)(isEnabled.Value ? 1 : 0);
            writer.WriteField((byte)RadioFrequencySetupField.IsEnabled, enabledFlag);
        }

        if (channel.HasValue)
        {
            writer.WriteField((byte)RadioFrequencySetupField.Channel, channel.Value);
        }

        if (band.HasValue)
        {
            writer.WriteField((byte)RadioFrequencySetupField.Band, band.Value);
        }

        if (attenuation.HasValue)
        {
            writer.WriteField((byte)RadioFrequencySetupField.Attenuation, attenuation.Value);
        }

        if (frequencyInMHz.HasValue)
        {
            writer.WriteField((byte)RadioFrequencySetupField.Frequency, frequencyInMHz.Value);
        }

        if (threshold.HasValue)
        {
            writer.WriteField((byte)RadioFrequencySetupField.Threshold, threshold.Value);
        }

        writer.Write(LapRFProtocol.END_OF_RECORD);

        byte[] finalisedData = memoryStream.FinalisePacketData();

        return _lapRFCommunication.SendAsync(finalisedData, token);
    }
    public async ValueTask<IEnumerable<RFSetup>> GetSetupAsync(IEnumerable<byte> laneIds, CancellationToken cancellationToken = default)
    {
        List<TaskCompletionSource<RFSetup>> responders = [];

        foreach (byte laneId in laneIds)
        {
            TaskCompletionSource<RFSetup> listner = AddListenerForLaneId(laneId, cancellationToken);
            responders.Add(listner);
        }

        //Build data
        using MemoryStream memoryStream = new();
        using BinaryWriter writer = new(memoryStream);

        writer.Write(LapRFProtocol.START_OF_RECORD);
        writer.WriteRecordType(RecordType.LAPRF_TOR_RFSETUP);
        foreach (byte laneId in laneIds)
        {
            writer.WriteField((byte)RadioFrequencySetupField.TransponderId, LapRFProtocol.GetTransponderId(laneId));
        }

        writer.Write(LapRFProtocol.END_OF_RECORD);

        byte[] finalisedData = memoryStream.FinalisePacketData();

        await _lapRFCommunication.SendAsync(finalisedData, cancellationToken).ConfigureAwait(false);

        await Task.WhenAll(responders.Select(x => x.Task)).ConfigureAwait(false);

        List<RFSetup> results = [];

        foreach (TaskCompletionSource<RFSetup> responder in responders)
        {
            RFSetup result = await responder.Task;
            results.Add(result);
        }

        return results;
    }
    private TaskCompletionSource<RFSetup> AddListenerForLaneId(byte laneId, CancellationToken cancellationToken)
    {
        TaskCompletionSource<RFSetup> responderCompletionSource = new(TaskCreationOptions.RunContinuationsAsynchronously);

        cancellationToken.Register(() => responderCompletionSource.TrySetCanceled());

        _responders.AddOrUpdate(laneId, [responderCompletionSource], (key, oldValue) =>
        {
            oldValue.Add(responderCompletionSource);
            return oldValue;
        });

        return responderCompletionSource;
    }
}
