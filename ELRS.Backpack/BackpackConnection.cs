using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.IO.Ports;
using System.Text;

namespace ELRS.Backpack;

internal class BackpackConnection : IBackpackConnection
{
    private const int _baudRate = 115200;
    private readonly string _commPort;
    private SerialPort? _serialPort;

    private readonly ConcurrentQueue<(byte[]? Uid, BackpackCommand command, TaskCompletionSource<byte[]> response)> _commandQueue = new();

    public BackpackConnection(string commPort)
    {
        _commPort = commPort ?? throw new ArgumentNullException(nameof(commPort));
    }

    public Task ConnectAsync(CancellationToken token)
    {
        try
        {
            _serialPort = new SerialPort(_commPort, _baudRate);
            _serialPort.Open();
        }
        catch
        {
            _serialPort?.Dispose();
            _serialPort = null;
            throw;
        }

        return Task.CompletedTask;
    }

    private static void ThrowIfNotConnected([NotNull] SerialPort? connection)
    {
        if (connection is null)
        {
            throw new InvalidOperationException("Serial port not connected");
        }
    }
    private Task<byte[]> SendWithResult(byte[]? UId, BackpackCommand command)
    {
        ThrowIfNotConnected(_serialPort);
        var completionSource = new TaskCompletionSource<byte[]>();
        _commandQueue.Enqueue((UId, command, completionSource));
        return completionSource.Task;
    }
    private Task Send(byte[]? UId, BackpackCommand command)
    {
        ThrowIfNotConnected(_serialPort);
        var completionSource = new TaskCompletionSource<byte[]>();
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

        byte[] responsePayload = await SendWithResult(UId, command);

        if (responsePayload.Length != 1)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return responsePayload[0];
    }
    public Task<ushort> GetBatteryVoltage(byte[] UId)
    {
        ThrowIfNotConnected(_serialPort);
        BackpackCommand command = new()
        {
            Function = BackpackCommands.GetBatteryVoltage,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
        };
        byte[] responsePayload = SendWithResult(UId, command).Result;

        if (responsePayload.Length != 2)
        {
            throw new InvalidOperationException("Invalid response length");
        }

        return Task.FromResult(BitConverter.ToUInt16(responsePayload));
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

        byte[] responsePayload = await SendWithResult(UId, command);

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
        byte[] responsePayload = await SendWithResult(UId, command);
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
        byte[] responsePayload = await SendWithResult(UId, command);
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
        byte[] responsePayload = await SendWithResult(null, command);
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
        byte[] responsePayload = await SendWithResult(null, command);

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
        byte[] responsePayload = await SendWithResult(UId, command);
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
        byte[] responsePayload = await SendWithResult(UId, command);
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
        ThrowIfNotConnected(_serialPort);
        BackpackCommand backpackCommand = new()
        {
            Function = BackpackCommands.SetOSDElement,
            Type = CommandType.Request,
            ShouldAwaitResponse = true,
            Payload = [(byte)command, (byte)row, (byte)column, (byte)presentation, .. Encoding.ASCII.GetBytes(message ?? string.Empty)],
        };
        return Send(UId, backpackCommand);
    }

    public async Task SetOSDElement(byte[] UId, string message, OSDPresentation presentation, byte row, byte column, TimeSpan duration)
    {
        await SendOSDCommand(UId, DisplayportCommand.ClearScreen, null, OSDPresentation.None, row, column);
        await SendOSDCommand(UId, DisplayportCommand.WriteString, message, presentation, row, column);
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
