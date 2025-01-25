using NaeTime.Bytes;

namespace NaeTime.Hardware.Node.Esp32.Abstractions;
public interface INodeTimingProtocol
{
    Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken);

    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
}
