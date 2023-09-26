namespace ImmersionRC.LapRF.Protocol;
public interface IPassingRecordProtocol
{
    Task<Pass?> WaitForNextPassAsync(CancellationToken cancellationToken);
    internal void HandleRecordData(ReadOnlySpanReader<byte> recordReader);
}
