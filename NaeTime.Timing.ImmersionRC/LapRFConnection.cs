using ImmersionRC.LapRF.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions.Notifications;

namespace NaeTime.Timing.ImmersionRC;
public class LapRFConnection
{
    private readonly ILapRFCommunication _communication;
    private readonly ILapRFProtocol _protocol;
    private readonly IDispatcher _dispatcher;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _connectionTask;
    public LapRFConnection(Guid timerId, IDispatcher dispatcher, ILapRFCommunication communication, ILapRFProtocol protocol)
    {
        _timerId = timerId;
        _dispatcher = dispatcher;
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

        _cancellationTokenSource = new CancellationTokenSource();

        _connectionTask = Run(_cancellationTokenSource.Token);
    }

    private async Task Run(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _communication.ConnectAsync();
                await _dispatcher.Dispatch(new TimerConnected(_timerId));

                await _protocol.RunAsync(token);
            }
            catch
            {
            }
            await _dispatcher.Dispatch(new TimerConnectionLost(_timerId));
        }
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return _connectionTask;
    }
}
