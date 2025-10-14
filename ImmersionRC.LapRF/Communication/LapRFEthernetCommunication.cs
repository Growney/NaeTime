using ImmersionRC.LapRF.Abstractions;
using System.Net;
using System.Net.Sockets;

namespace ImmersionRC.LapRF.Communication;
internal class LapRFEthernetCommunication(IPAddress address, int port) : ILapRFCommunication
{
    private TcpClient? _client = new TcpClient();
    private readonly byte[] _rxBuffer = new byte[1024];

    private readonly IPAddress _address = address;
    private readonly int _port = port;

    public async Task ConnectAsync(CancellationToken token)
    {
        _client?.Dispose();

        _client = new TcpClient();
        await _client.ConnectAsync(_address, _port, token).ConfigureAwait(false);
    }
    public Task DisconnectAsync(CancellationToken token)
    {
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }

        return Task.CompletedTask;
    }

    public async Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Not connected");
        }

        NetworkStream stream = _client.GetStream();

        int readBytes = await stream.ReadAsync(_rxBuffer, 0, _rxBuffer.Length).ConfigureAwait(false);

        return _rxBuffer.AsMemory(0, readBytes);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken token)
    {
        if (_client == null)
        {
            throw new InvalidOperationException("Not connected");
        }

        NetworkStream stream = _client.GetStream();

        return stream.WriteAsync(data, token);
    }
}
