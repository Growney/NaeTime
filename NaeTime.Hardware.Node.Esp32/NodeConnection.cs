using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Messages;
using NaeTime.Hardware.Node.Esp32.Abstractions;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnection
{
    private readonly INodeCommunication _communication;
    private readonly INodeProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IEventClient _eventClient;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private readonly Task[] _runningTasks;

    public NodeConnection(Guid timerId, ISoftwareTimer softwareTimer, IEventClient eventClient, INodeCommunication communication, INodeProtocol protocol)
    {
        _timerId = timerId;
        _eventClient = eventClient;
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

        _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = _cancellationTokenSource.Token;

        _runningTasks = [MaintainConnectionAsync(token), WaitForRSSIAsync(token), WaitForPassAsync(token)];
    }

    private async Task MaintainConnectionAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _communication.ConnectAsync(token).ConfigureAwait(false);
                IsConnected = true;

                //We must start the run task before we dispatch the connection established as data may be requested when the connection is established
                System.Runtime.CompilerServices.ConfiguredTaskAwaitable runTask = _protocol.RunAsync(token).ConfigureAwait(false);

                await _eventClient.PublishAsync(new TimerConnectionEstablished(_timerId, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);

                await runTask;
            }
            catch
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            if (IsConnected)
            {
                IsConnected = false;
                await _eventClient.PublishAsync(new TimerDisconnected(_timerId, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
            }

            await _communication.DisconnectAsync(token).ConfigureAwait(false);
        }
    }

    private async Task WaitForRSSIAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                ReceivedSignalStrengthIndicator? rssi = await _protocol.TimingProtocol.WaitForNextReceivedSignalStrengthIndicatorAsync(token).ConfigureAwait(false);

                if (rssi == null)
                {
                    continue;
                }

                ReceivedSignalStrengthIndicator status = rssi.Value;

                RssiLevelRecorded level = new(_timerId, status.Lane, status.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, status.Level);

                await _eventClient.PublishAsync(level).ConfigureAwait(false);
            }
            catch
            {

            }
        }
    }

    private async Task WaitForPassAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                Pass? nullablePassingRecord = await _protocol.TimingProtocol.WaitForNextPassAsync(token).ConfigureAwait(false);
                if (nullablePassingRecord == null)
                {
                    continue;
                }
                Pass passingRecord = nullablePassingRecord.Value;
                TimerDetectionOccured detection = new(_timerId, passingRecord.Lane, passingRecord.Time, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow);
                await _eventClient.PublishAsync(detection).ConfigureAwait(false);
            }
            catch
            {
            }
        }
    }
    public async Task SetLaneRadioFrequency(byte Lane, int frequencyInMhz)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.ConfigurationProtocol.SetLaneFrequency(Lane, (ushort)frequencyInMhz).ConfigureAwait(false);
    }

    public async Task SetLaneEntryThreshold(byte Lane, ushort threshold)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.ConfigurationProtocol.SetEntryThreshold(Lane, threshold).ConfigureAwait(false);
    }

    public async Task SetLaneExitThreshold(byte Lane, ushort threshold)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.ConfigurationProtocol.SetExitThreshold(Lane, threshold).ConfigureAwait(false);
    }

    public async Task SetLaneEnabled(byte Lane, bool isEnabled)
    {
        if (!IsConnected)
        {
            return;
        }
        await _protocol.ConfigurationProtocol.SetLaneEnabled(Lane, isEnabled).ConfigureAwait(false);
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return Task.WhenAll(_runningTasks);
    }
}
