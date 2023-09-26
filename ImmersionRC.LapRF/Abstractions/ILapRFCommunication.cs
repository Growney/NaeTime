namespace ImmersionRC.LapRF.Abstractions;
internal interface ILapRFCommunication
{
    ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken token);
    Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token);
}
