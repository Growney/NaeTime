using NaeTime.Collections;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Text;

namespace ELRS.Backpack;

internal class BackpackConnection : IBackpackConnection
{
    private const int _baudRate = 460800;
    private readonly string _commPort;
    private SerialPort? _serialPort;

    private readonly AwaitableQueue<(byte[]? Uid, BackpackCommand command, TaskCompletionSource<BackpackCommand?> response)> _commandQueue = new(1000);
    private readonly ConcurrentDictionary<BackpackCommands, List<TaskCompletionSource<BackpackCommand?>>> _responseQueue = new();
    private readonly IBackpackCommandSerializer _serializer;

    //Set it to something so that it doesn't match and empty or a valid Uid and gets set on the first command
    private byte[] _currentUId = [1];

    public BackpackConnection(string commPort, IBackpackCommandSerializer serializer)
    {
        _commPort = commPort ?? throw new ArgumentNullException(nameof(commPort));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public Task ConnectAsync(CancellationToken token)
    {
        try
        {
            _serialPort = new SerialPort(portName: _commPort,
                baudRate: _baudRate,
                parity: Parity.None,
                stopBits: StopBits.One,
                dataBits: 8);
            _serialPort.DtrEnable = true;

            _serialPort.Open();
        }
        catch
        {
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;
            throw;
        }

        return Task.CompletedTask;
    }

    public async Task Run(CancellationToken token)
    {
        Task[] tasks = [RunCommandLoop(token), RunReceiveLoop(token)];

        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        finally
        {
            _serialPort?.BaseStream.Close();
            _serialPort?.Close();
            _serialPort?.Dispose();
            _serialPort = null;
        }
    }

    private async Task RunCommandLoop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                if (_serialPort == null)
                {
                    break;
                }

                (byte[]? UId, BackpackCommand command, TaskCompletionSource<BackpackCommand?> response) = await _commandQueue.WaitForDequeueAsync(token).ConfigureAwait(false);

                if (command == null || token.IsCancellationRequested)
                {
                    continue;
                }

                byte[] commandUId = UId ?? Array.Empty<byte>();
                if (!_currentUId.SequenceEqual(commandUId))
                {
                    BackpackCommand uidCommand = CreateUIdCommand(commandUId);
                    await SendCommand(uidCommand).ConfigureAwait(false);
                    _currentUId = commandUId;
                }

                if (command.ShouldAwaitResponse)
                {
                    List<TaskCompletionSource<BackpackCommand?>> responseSources = _responseQueue.GetOrAdd(command.Function, _ => new List<TaskCompletionSource<BackpackCommand?>>());
                    responseSources.Add(response);
                }

                await SendCommand(command).ConfigureAwait(false);
                if (!command.ShouldAwaitResponse)
                {
                    response.TrySetResult(null);
                }
                await Task.Delay(250);
            }
            catch (Exception)
            {
            }
            finally
            {

            }

        }
    }
    private async Task RunReceiveLoop(CancellationToken token)
    {
        ThrowIfNotConnected(_serialPort);

        AwaitableQueue<byte> receivedQueue = new(512);

        void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                if (_serialPort == null)
                {
                    return;
                }
                while (_serialPort.BytesToWrite > 0 && !token.IsCancellationRequested)
                {
                    receivedQueue.Enqueue((byte)_serialPort.ReadByte());
                }
            }
            catch
            {

            }
        }

        while (!token.IsCancellationRequested)
        {
            try
            {
                byte[] data = await receivedQueue.WaitForDequeueAsync(BackpackCommand.HeaderSize, token).ConfigureAwait(false);

                if (data[(int)BackpackCommand.HeaderLocation.Start] != BackpackCommand.StartingCharacter
                    || data[(int)BackpackCommand.HeaderLocation.Version] != BackpackCommand.Version
                    || data[(int)BackpackCommand.HeaderLocation.Type] != (byte)CommandType.Response)
                {
                    continue;
                }

                if (token.IsCancellationRequested)
                {
                    break;
                }

                byte flag = data[(int)BackpackCommand.HeaderLocation.Flag];
                BackpackCommands command = (BackpackCommands)data[(int)BackpackCommand.HeaderLocation.Function];
                byte[] payloadSizeBytes = data.AsSpan().Slice((int)BackpackCommand.HeaderLocation.PayloadSize, 2).ToArray();
                ushort payloadSize = BitConverter.ToUInt16(payloadSizeBytes);

                byte[] payloadData = await receivedQueue.WaitForDequeueAsync(payloadSize + BackpackCommand.CrcSize, token).ConfigureAwait(false);

                if (token.IsCancellationRequested)
                {
                    break;
                }
                byte[] payload = payloadData.AsSpan().Slice(BackpackCommand.HeaderSize, payloadSize).ToArray();
                byte crc = payloadData[BackpackCommand.HeaderSize + payloadSize];

                //TODO Add CRC check

                BackpackCommand backpackCommand = new()
                {
                    Function = command,
                    Flag = flag,
                    Type = CommandType.Response,
                    Payload = payload,
                };

                if (_responseQueue.TryGetValue(command, out var tasksWaitingForResponse))
                {
                    foreach (TaskCompletionSource<BackpackCommand?> task in tasksWaitingForResponse)
                    {
                        task.TrySetResult(backpackCommand);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {

            }
        }
    }

    private BackpackCommand CreateUIdCommand(byte[] uId) => new()
    {
        Function = BackpackCommands.SetUid,
        Payload = [1, .. uId]
    };
    private BackpackCommand CreateClearUId() => new()
    {
        Function = BackpackCommands.SetUid,
        Payload = [0],
    };

    private Task SendCommand(BackpackCommand command)
    {
        ThrowIfNotConnected(_serialPort);
        byte[] bytes = _serializer.Serialize(command);
        return _serialPort.BaseStream.WriteAsync(bytes, 0, bytes.Length);
    }

    private static void ThrowIfNotConnected([NotNull] SerialPort? connection)
    {
        if (connection is null)
        {
            throw new InvalidOperationException("Serial port not connected");
        }
    }
    private Task<BackpackCommand> SendWithResult(byte[]? UId, BackpackCommand command)
    {
        ThrowIfNotConnected(_serialPort);
        var completionSource = new TaskCompletionSource<BackpackCommand>();
        _commandQueue.Enqueue((UId, command, completionSource));
        return completionSource.Task;
    }
    private Task Send(byte[]? UId, BackpackCommand command)
    {
        ThrowIfNotConnected(_serialPort);
        TaskCompletionSource<BackpackCommand> completionSource = new();
        _commandQueue.Enqueue((UId, command, completionSource));
        return completionSource.Task;
    }
    public async Task<byte> GetBandChannelIndex(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);

        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetBandChannelIndex,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 1)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload[0];
    }
    public async Task<ushort> GetBatteryVoltage(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetBatteryVoltage,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 2)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return BitConverter.ToUInt16(responsePayload);
    }
    public async Task<ushort> GetFrequency(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetFrequency,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 2)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return BitConverter.ToUInt16(responsePayload);
    }
    public async Task<bool> GetRecordingState(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetRecordingState,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 1)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload[0] == 1;
    }
    public async Task<byte> GetRSSI(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetRSSI,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 1)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload[0];
    }
    public async Task<BackpackStatus> GetStatus()
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetStatus,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(null, command);
        byte[] responsePayload = responseCommand.Payload;
        if (responsePayload.Length != 7)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return new BackpackStatus(
            (responsePayload[0] & 0x01) != 0,
            (responsePayload[0] & 0x02) != 0,
            (responsePayload[0] & 0x04) != 0,
            responsePayload[1..]);
    }
    public async Task<string> GetVersion()
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetBackpackVersion,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(null, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 2)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return System.Text.Encoding.ASCII.GetString(responsePayload);
    }
    public async Task<byte> GetVRxMode(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetVRxMode,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 1)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload[0];
    }
    public async Task<byte[]> GetVRxVersion(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetVRxVersion,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };

        BackpackCommand responseCommand = await SendWithResult(UId, command);
        byte[] responsePayload = responseCommand.Payload;

        if (responsePayload.Length != 2)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload;
    }
    public Task SetBandChannelIndex(byte[] UId, byte channelIndex)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetBandChannelIndex,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [channelIndex],
        };
        return Send(UId, command);
    }
    public Task SetBuzzer(byte[] UId, ushort durationMs)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetBuzzer,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = BitConverter.GetBytes(durationMs),
        };
        return Send(UId, command);
    }
    public Task SetFrequency(byte[] UId, ushort frequencyInMHz)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetFrequency,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = BitConverter.GetBytes(frequencyInMHz),
        };
        return Send(UId, command);
    }
    public Task SetHeadTrackingData(byte[] UId, ushort pan, ushort tilt, ushort roll)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetHeadTrackingData,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [(byte)(pan >> 8), (byte)pan, (byte)(tilt >> 8), (byte)tilt, (byte)(roll >> 8), (byte)roll],
        };
        return Send(UId, command);
    }
    public Task SetHeadTrackingState(byte[] UId, bool state)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetHeadTrackingState,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [state ? (byte)1 : (byte)0],
        };
        return Send(UId, command);
    }
    public Task SetMode(BackpackMode mode)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetMode,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [(byte)mode],
        };
        return Send(null, command);
    }
    private Task SendOSDCommand(byte[] UId, DisplayportCommand command, string? message, OSDPresentation presentation, byte row, byte column)
    {
        byte[] payload = command switch
        {
            DisplayportCommand.WriteString => [(byte)command, (byte)row, (byte)column, (byte)presentation, .. Encoding.ASCII.GetBytes(message ?? string.Empty)],
            _ => [(byte)command]
        };

        ThrowIfNotConnected(_serialPort);
        BackpackCommand backpackCommand = new()
        {
            Function = BackpackCommands.SetOSDElement,
            Type = CommandType.Request,
            ShouldAwaitResponse = false,
            Payload = payload,
        };
        return Send(UId, backpackCommand);
    }
    public async Task SetOSDElement(byte[] UId, string message, OSDPresentation presentation, byte row, byte column, TimeSpan duration)
    {
        await SendOSDCommand(UId, DisplayportCommand.ClearScreen, null, OSDPresentation.None, row, column);
        await SendOSDCommand(UId, DisplayportCommand.WriteString, message.ToUpper(), presentation, row, column);
        await SendOSDCommand(UId, DisplayportCommand.DrawScreen, null, OSDPresentation.None, row, column);
        await Task.Delay(duration);
        await SendOSDCommand(UId, DisplayportCommand.ClearScreen, null, OSDPresentation.None, row, column);
        await SendOSDCommand(UId, DisplayportCommand.DrawScreen, null, OSDPresentation.None, row, column);
    }
    public Task SetRecordingState(byte[] UId, bool recording)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetRecordingState,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [recording ? (byte)1 : (byte)0],
        };
        return Send(UId, command);
    }
    public Task SetVRxMode(byte[] UId, byte mode)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.SetVRxMode,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [mode],
        };
        return Send(UId, command);
    }
}
