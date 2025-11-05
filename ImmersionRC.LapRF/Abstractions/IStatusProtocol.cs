using NaeTime.Bytes;

namespace ImmersionRC.LapRF.Abstractions;
public interface IStatusProtocol
{
    Status? CurrentStatus { get; }
    TimeSpan? CurrentStatusAge { get; }
    ReceivedSignalStrengthIndicator? GetLastReceivedSignalStrengthIndicator(byte laneId);
    Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken);

    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
}
