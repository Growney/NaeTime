namespace ImmersionRC.LapRF.Abstractions;
public interface IPassingRecordProtocol
{
    Task<Pass?> WaitForNextPassAsync(CancellationToken cancellationToken);
    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
}
