using ImmersionRC.LapRF.Abstractions;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Abstractions;
using NaeTime.Timing.Abstractions.Models;
using NaeTime.Timing.Abstractions.Notifications;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnection
{
    private readonly ILapRFCommunication _communication;
    private readonly ILapRFProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IDispatcher _dispatcher;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private readonly Task[] _runningTasks;

    public LapRFConnection(Guid timerId, ISoftwareTimer softwareTimer, IDispatcher dispatcher, ILapRFCommunication communication, ILapRFProtocol protocol)
    {
        _timerId = timerId;
        _dispatcher = dispatcher;
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

        _cancellationTokenSource = new CancellationTokenSource();

        var token = _cancellationTokenSource.Token;

        _runningTasks = [MaintainConnectionAsync(token), WaitForDetectionsAsync(token), WaitForStatusAsync(token)];
    }

    private async Task MaintainConnectionAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _communication.ConnectAsync();
                IsConnected = true;
                await _dispatcher.Dispatch(new TimerConnected(_timerId));

                await _protocol.RunAsync(token);
            }
            catch
            {
                await Task.Delay(500);
            }
            if (IsConnected)
            {
                IsConnected = false;
                await _dispatcher.Dispatch(new TimerConnectionLost(_timerId));
            }
        }

    }
    private async Task WaitForDetectionsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var nullablePassingRecord = await _protocol.PassingRecordProtocol.WaitForNextPassAsync(token).ConfigureAwait(false);

                if (nullablePassingRecord == null)
                {
                    continue;
                }

                var passingRecord = nullablePassingRecord.Value;

                var detection = new Detection(Guid.NewGuid(), passingRecord.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, _timerId, passingRecord.PilotId);

                await _dispatcher.Dispatch(new DetectionOccured(detection)).ConfigureAwait(false);
            }
            catch
            {

            }
        }
    }
    private async Task WaitForStatusAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                var nullableStatus = await _protocol.StatusProtocol.WaitForNextReceivedSignalStrengthIndicatorAsync(token).ConfigureAwait(false);

                if (nullableStatus == null)
                {
                    continue;
                }

                var status = nullableStatus.Value;

                var level = new RssiLevel(Guid.NewGuid(), status.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, _timerId, status.TransponderId, status.Level);

                await _dispatcher.Dispatch(new RssiLevelRecorded(level)).ConfigureAwait(false);
            }
            catch
            {

            }
        }
    }

    public async Task<IEnumerable<TimerRadioFrequencyChannel>> GetRadioFrequencyChannelsAsync()
    {
        if (!IsConnected)
        {
            return Enumerable.Empty<TimerRadioFrequencyChannel>();
        }

        byte[] transponderIds = [1, 2, 3, 4, 5, 6, 7, 8];

        var rfSetups = await _protocol.RadioFrequencySetupProtocol.GetSetupAsync(transponderIds, CancellationToken.None).ConfigureAwait(false);

        var channels = new List<TimerRadioFrequencyChannel>();

        foreach (var setup in rfSetups)
        {
            if (setup.Frequency == null)
            {
                continue;
            }
            channels.Add(new TimerRadioFrequencyChannel(setup.TransponderId, (int)setup.Frequency.Value, setup.IsEnabled));
        }

        return channels;
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return Task.WhenAll(_runningTasks);
    }
}
