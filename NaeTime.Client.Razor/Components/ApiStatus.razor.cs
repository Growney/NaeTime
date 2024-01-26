using Microsoft.AspNetCore.Components;
using NaeTime.Client.Razor.Lib.Abstractions;

namespace NaeTime.Client.Razor.Components;
public partial class ApiStatus : IDisposable
{
    private enum Status
    {
        Ok,
        Warning,
        Disconnected,
    }
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;

    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    [Parameter]
    [EditorRequired]
    public IApiClientProvider ClientProvider { get; set; } = null!;

    [Parameter]
    public TimeSpan WarningTimeSince { get; set; } = TimeSpan.FromSeconds(3);
    [Parameter]
    public TimeSpan DisconnectedTimeSince { get; set; } = TimeSpan.FromSeconds(10);

    private DateTime _lastConnectionSuccess = DateTime.MinValue;
    private Status _status;
    private CancellationTokenSource? _cancelSource;

    private string GetContentClass()
        => _status switch
        {
            Status.Ok => "text-success",
            Status.Warning => "text-warning",
            Status.Disconnected => "text-danger",
            _ => string.Empty
        };

    private async Task PeriodicallyCheckStatus(CancellationToken token)
    {
        using var periodicTimer = new PeriodicTimer(WarningTimeSince);

        while (!token.IsCancellationRequested)
        {
            bool connected = await ClientProvider.TryConnectionAsync(token);
            if (connected)
            {
                _lastConnectionSuccess = DateTime.UtcNow;
            }

            if (DateTime.UtcNow - _lastConnectionSuccess > DisconnectedTimeSince)
            {
                _status = Status.Disconnected;
            }
            else if (DateTime.UtcNow - _lastConnectionSuccess > WarningTimeSince)
            {
                _status = Status.Warning;
            }
            else
            {
                _status = Status.Ok;
            }

            await periodicTimer.WaitForNextTickAsync(token);
        }
    }
    protected override void OnInitialized()
    {
        _cancelSource = new CancellationTokenSource();

        base.OnInitialized();
    }

    public void Dispose()
    {
        _cancelSource?.Cancel();
    }

}