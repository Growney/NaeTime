using ELRS.Backpack;
using NaeTime.Hardware.ELRS.Abstractions;

namespace NaeTime.Hardware.ELRS;

public class BackpackConnector : IBackpackConnector
{
    private readonly IBackpackConnection _connection;
    private readonly Guid _connectorId;
    private readonly CancellationTokenSource _cancellationTokenSource;

    private readonly Task _finishingTask;

    public bool IsConnected { get; private set; }

    public BackpackConnector(Guid connectorId, IBackpackConnection connection)
    {
        _connection = connection ?? throw new ArgumentNullException(nameof(connection));
        _connectorId = connectorId;

        _cancellationTokenSource = new CancellationTokenSource();

        _finishingTask = Run(_cancellationTokenSource.Token);
    }

    private Task Run(CancellationToken token)
    {
        Task[] tasks = [MaintainConnection(token)];

        return Task.WhenAll(tasks);
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
        _cancellationTokenSource.Cancel();

        return _finishingTask;
    }

    public async Task SetOSDElement(byte[] uid, string text, byte row, byte column, TimeSpan duration)
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("Not connected");
        }
        _ = _connection.SetOSDElement(uid, text, OSDPresentation.None, row, column, duration);
    }
}
