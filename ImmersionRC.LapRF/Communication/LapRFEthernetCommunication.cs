using ImmersionRC.LapRF.Abstractions;
using System.Net;
using System.Net.Sockets;

namespace ImmersionRC.LapRF.Communication;
internal class LapRFEthernetCommunication : ILapRFCommunication
{
    private readonly TcpClient _client;
    private readonly byte[] _rxBuffer;

    private readonly IPAddress _address;
    private readonly int _port;

    public LapRFEthernetCommunication(IPAddress address, int port)
    {
        _address = address;
        _port = port;

        _client = new TcpClient();
        _rxBuffer = new byte[1024];

    }

    public async Task ConnectAsync(CancellationToken token)
        => await _client.ConnectAsync(_address, _port, token);


    public async Task<ReadOnlyMemory<byte>> ReceiveAsync(CancellationToken token)
    {
        var stream = _client.GetStream();

        int readBytes = await stream.ReadAsync(_rxBuffer, 0, _rxBuffer.Length);

        return _rxBuffer.AsMemory(0, readBytes);
    }

    public ValueTask SendAsync(ReadOnlyMemory<byte> data, CancellationToken token)
    {
        var stream = _client.GetStream();

        return stream.WriteAsync(data, token);
    }
}
