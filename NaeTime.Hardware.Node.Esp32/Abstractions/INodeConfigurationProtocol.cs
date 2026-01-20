namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeConfigurationProtocol : INodeSubProtocol
{
    ValueTask<bool> SetLaneFrequency(byte lane,byte? bandId, ushort frequencyInMHz, CancellationToken token = default);
    ValueTask<bool> SetEntryThreshold(byte lane, ushort threshold, CancellationToken token = default);
    ValueTask<bool> SetExitThreshold(byte lane, ushort threshold, CancellationToken token = default);
    ValueTask<bool> SetLaneEnabled(byte lane, bool isEnabled, CancellationToken token = default);

    ValueTask<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfiguration(IEnumerable<byte> laneIds);
}
