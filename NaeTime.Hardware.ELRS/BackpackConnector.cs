using ELRS.Backpack;
using NaeTime.Hardware.ELRS.Abstractions;

namespace NaeTime.Hardware.ELRS;

public class BackpackConnector : IBackpackConnector
{
    private readonly IBackpackConnection _connection;
    private readonly Guid _connectorId;
    private CancellationTokenSource? _cancellationTokenSource;

    private Task[] _runningTasks;

    public bool IsConnected { get; private set; }

    public BackpackConnector(Guid connectorId, IBackpackConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _connectorId = connectorId;

        _runningTasks = Array.Empty<Task>();
    }

    public Task Start()
    {

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = _cancellationTokenSource.Token;
        _runningTasks = [MaintainConnection(token)];

        return Task.CompletedTask;
    }

    private async Task MaintainConnection(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _connection.ConnectAsync(token).ConfigureAwait(false);
                IsConnected = true;

                await _connection.Run(token).ConfigureAwait(false);
            }
            catch
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
        }
    }

    public Task Stop()
    {
        _cancellationTokenSource?.Cancel();

        return Task.WhenAll(_runningTasks);
    }

    public async Task SetOSDElement(byte[] uid, string text, byte row, byte column, TimeSpan? duration = null)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }
        _ = _connection.SetOSDElement(uid, text, OSDPresentation.None, row, column, duration);
    }
}
