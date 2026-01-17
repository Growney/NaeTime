using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;
namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnection : INodeConnection
{
    private readonly INodeCommunication _communication;
    private readonly INodeProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IStreamEventWriter _writer;
    private readonly INaeTimeNodeCommandHandler _commandHandler;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private readonly Task[] _runningTasks;

    private readonly string _detectionsStream;
    private readonly string _rssiStream;

    public NodeConnection(Guid timerId, ISoftwareTimer softwareTimer, INodeCommunication communication, INodeProtocol protocol, IStreamEventWriter writer, INaeTimeNodeCommandHandler commandHandler)
    {
        _timerId = timerId;

        _detectionsStream = $"NaeTime-Node-{_timerId}-Detections";
        _rssiStream = $"NaeTime-Node-{_timerId}-Rssi";

        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));
        _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        _commandHandler = commandHandler ?? throw new ArgumentNullException(nameof(commandHandler));

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
                await _commandHandler.MarkAsConnected(_timerId).ConfigureAwait(false);

                //We must start the run task before we dispatch the connection established as data may be requested when the connection is established
                System.Runtime.CompilerServices.ConfiguredTaskAwaitable runTask = _protocol.RunAsync(token).ConfigureAwait(false);

                await runTask;
            }
            catch
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }

            if (IsConnected)
            {
                await _commandHandler.MarkAsDisconnected(_timerId).ConfigureAwait(false);
                IsConnected = false;
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

                await _writer.AppendToStream(_rssiStream, new RssiRecorded(_timerId, status.Lane, status.Level, _softwareTimer.ElapsedMilliseconds, status.RealTimeClockTime));
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

                await _writer.AppendToStream(_detectionsStream, new NaeTime.Events.HardwareDetectionOccured(Guid.NewGuid(), _timerId, passingRecord.Lane, passingRecord.Time, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
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
