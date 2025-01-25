
using NaeTime.Hardware.Node.Esp32.Abstractions;
using System.IO.Ports;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeSerialCommunication : INodeCommunication
{
    private readonly string _commPort;
    private readonly byte[] _rxBuffer;
    private SerialPort? _serialPort;

    public NodeSerialCommunication(string commPort)
    {
        _commPort = commPort ?? throw new ArgumentNullException(nameof(commPort));
        _rxBuffer = new byte[1024];
    }

    public Task ConnectAsync(CancellationToken token)
    {
        _serialPort = new SerialPort(_commPort, 115200);
        _serialPort.Open();

        return Task.CompletedTask;
    }
    public Task DisconnectAsync(CancellationToken token)
    {
        _serialPort?.Close();
        return Task.CompletedTask;
    }
    async Task<ReadOnlyMemory<byte>> INodeCommunication.ReceiveAsync(CancellationToken token)
    {
        if (_serialPort is null)
        {
            throw new InvalidOperationException("Serial port is not open");
        }

        int readBytes = await _serialPort.BaseStream.ReadAsync(_rxBuffer, 0, _rxBuffer.Length);

        return _rxBuffer.AsMemory(0, readBytes);
    }
    ValueTask INodeCommunication.SendAsync(ReadOnlyMemory<byte> data, CancellationToken token)
    {
        if (_serialPort is null)
        {
            throw new InvalidOperationException("Serial port is not open");
        }
        _serialPort.BaseStream.Write(data.Span);
        return ValueTask.CompletedTask;
    }
}
