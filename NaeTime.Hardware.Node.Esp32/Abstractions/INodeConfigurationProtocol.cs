namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeConfigurationProtocol : INodeSubProtocol
{
    ValueTask SetLaneFrequency(byte lane, ushort frequencyInMHz, CancellationToken token = default);
    ValueTask SetEntryThreshold(byte lane, ushort threshold, CancellationToken token = default);
    ValueTask SetExitThreshold(byte lane, ushort threshold, CancellationToken token = default);
}
