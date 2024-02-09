using ImmersionRC.LapRF.Abstractions;
using System.Collections.Concurrent;

namespace ImmersionRC.LapRF.Protocol;
internal class RadioFrequencySetupProtocol : IRadioFrequencySetupProtocol
{
    private readonly ILapRFCommunication _lapRFCommunication;
    private ConcurrentDictionary<byte, RFSetup> _latestSetup = new();
    private ConcurrentDictionary<byte, List<TaskCompletionSource<RFSetup>>> _responders = new();
    private readonly Crc16 _crc16 = new();

    public RadioFrequencySetupProtocol(ILapRFCommunication lapRFCommunication)
    {
        _lapRFCommunication = lapRFCommunication;
    }

    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte? transponderId = null;
        ushort? isEnabled = null;
        ushort? channel = null;
        ushort? band = null;
        ushort? attenuation = null;
        ushort? frequency = null;

        while (recordReader.HasData())
        {
            var fieldSignature = recordReader.ReadByte();

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
                default:
                    break;
            }
        }

        if (transponderId.HasValue)
        {
            var setup = new RFSetup(transponderId.Value, isEnabled == 1, channel, band, attenuation, frequency);
            HandleTransponderSetup(setup);
        }
    }

    private void HandleTransponderSetup(RFSetup setup)
    {
        _latestSetup.AddOrUpdate(setup.TransponderId, setup,
                    (key, oldValue) => setup);

        if (_responders.TryGetValue(setup.TransponderId, out var responders))
        {
            foreach (var responder in responders)
            {
                responder.SetResult(setup);
            }
            responders.Clear();
        }
    }
    public ValueTask SetupTransponderSlot(byte transponderId, bool? isEnabled, ushort? channel = null, ushort? band = null, ushort? attenuation = null, ushort? frequencyInMHz = null, CancellationToken token = default)
    {
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        writer.Write(LapRFProtocol.START_OF_RECORD);
        writer.WriteRecordType(RecordType.LAPRF_TOR_RFSETUP);
        writer.WriteField((byte)RadioFrequencySetupField.TransponderId, transponderId);
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
        writer.Write(LapRFProtocol.END_OF_RECORD);

        var finalisedData = memoryStream.FinalisePacketData();

        return _lapRFCommunication.SendAsync(finalisedData, token);
    }
    public async ValueTask<IEnumerable<RFSetup>> GetSetupAsync(IEnumerable<byte> transponderIds, CancellationToken cancellationToken = default)
    {
        var responders = new List<TaskCompletionSource<RFSetup>>();

        foreach (var transponderId in transponderIds)
        {
            var listner = AddListenerForTransponderId(transponderId, cancellationToken);
            responders.Add(listner);
        }

        //Build data
        using var memoryStream = new MemoryStream();
        using var writer = new BinaryWriter(memoryStream);

        writer.Write(LapRFProtocol.START_OF_RECORD);
        writer.WriteRecordType(RecordType.LAPRF_TOR_RFSETUP);
        foreach (var transponderId in transponderIds)
        {
            writer.WriteField((byte)RadioFrequencySetupField.TransponderId, transponderId);
        }
        writer.Write(LapRFProtocol.END_OF_RECORD);

        var finalisedData = memoryStream.FinalisePacketData();

        await _lapRFCommunication.SendAsync(finalisedData, cancellationToken);

        await Task.WhenAll(responders.Select(x => x.Task));

        var results = new List<RFSetup>();

        foreach (var responder in responders)
        {
            var result = await responder.Task;
            results.Add(result);
        }


        return results;
    }
    private TaskCompletionSource<RFSetup> AddListenerForTransponderId(byte transponderId, CancellationToken cancellationToken)
    {
        var responderCompletionSource = new TaskCompletionSource<RFSetup>(TaskCreationOptions.RunContinuationsAsynchronously);

        cancellationToken.Register(() =>
        {
            responderCompletionSource.TrySetCanceled();
        });

        _responders.AddOrUpdate(transponderId, new List<TaskCompletionSource<RFSetup>>() { responderCompletionSource }, (key, oldValue) =>
        {
            oldValue.Add(responderCompletionSource);
            return oldValue;
        });

        return responderCompletionSource;
    }
}
