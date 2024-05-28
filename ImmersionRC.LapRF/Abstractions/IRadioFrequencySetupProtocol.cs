namespace ImmersionRC.LapRF.Abstractions;
public interface IRadioFrequencySetupProtocol
{
    ValueTask<IEnumerable<RFSetup>> GetSetupAsync(IEnumerable<byte> transponderIds, CancellationToken cancellationToken);
    ValueTask SetupTransponderSlot(byte transponderId, bool? isEnabled = null, ushort? channel = null, ushort? band = null, ushort? attenuation = null, ushort? frequencyInMHz = null, float? threshold = null, CancellationToken token = default);
    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
}
