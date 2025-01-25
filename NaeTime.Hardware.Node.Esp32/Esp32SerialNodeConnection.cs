using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Messages;
using NaeTime.PubSub.Abstractions;
using System.IO.Ports;

namespace NaeTime.Hardware.Node.Esp32;
public class Esp32SerialNodeConnection
{
    private readonly SerialPort _port;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IEventClient _eventClient;
    private readonly Guid _timerId;
    private readonly Task _readTask;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly byte[] _buffer;

    public Esp32SerialNodeConnection(Guid timerId, string portName, ISoftwareTimer softwareTimer, IEventClient eventClient, int bufferSize = 1024)
    {
        _timerId = timerId;

        _buffer = new byte[bufferSize];
        _port = new SerialPort(portName, 115200);
        _port.Open();

        _softwareTimer = softwareTimer;
        _eventClient = eventClient;

        _cancellationTokenSource = new CancellationTokenSource();
        _readTask = ProcessSerial(_cancellationTokenSource.Token);
    }



    private async Task ProcessSerial(CancellationToken token)
    {
        await Task.Delay(1000);
        using BinaryReader reader = new BinaryReader(_port.BaseStream);
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_port.BytesToRead > 0)
                {
                    byte command = reader.ReadByte();

                    if (command == 0x03)
                    {
                        //Lane timings

                        int crc = reader.ReadInt32();
                        int dataLength = reader.ReadInt32();
                        byte[] data = reader.ReadBytes(dataLength);

                        using MemoryStream dataStream = new MemoryStream(data);
                        using BinaryReader dataReader = new BinaryReader(dataStream);

                        byte lane = dataReader.ReadByte();
                        ulong time = (ulong)dataReader.ReadInt32();
                        ushort rssi = dataReader.ReadUInt16();
                        long lastPassStart = dataReader.ReadInt32();
                        long lastPassEnd = dataReader.ReadInt32();
                        short passState = dataReader.ReadInt16();
                        int passCount = dataReader.ReadInt32();
                        System.Diagnostics.Debug.WriteLine($"Lane: {lane}");
                        System.Diagnostics.Debug.WriteLine($"Time: {time}");
                        System.Diagnostics.Debug.WriteLine($"Rssi: {rssi}");
                        System.Diagnostics.Debug.WriteLine($"Last Pass Start: {lastPassStart}");
                        System.Diagnostics.Debug.WriteLine($"Last Pass End: {lastPassEnd}");
                        System.Diagnostics.Debug.WriteLine($"Pass State: {passState}");
                        System.Diagnostics.Debug.WriteLine($"Pass Count: {passCount}");
                        RssiLevelRecorded level = new(_timerId, lane, time, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, rssi);

                        _ = _eventClient.PublishAsync(level).ConfigureAwait(false);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Command not 3");
                    }
                }
                else
                {
                    await Task.Delay(100);
                }
            }
            catch
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }

    public async Task Stop()
    {
        _cancellationTokenSource.Cancel();
        await _readTask.ConfigureAwait(false);
        _port.Close();
    }
}
