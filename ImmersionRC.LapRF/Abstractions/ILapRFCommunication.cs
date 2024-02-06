namespace ImmersionRC.LapRF.Abstractions;
public interface ILapRFCommunication
{
    public Task ConnectAsync(CancellationToken token);

    internal ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken token);
    internal Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token);
}
