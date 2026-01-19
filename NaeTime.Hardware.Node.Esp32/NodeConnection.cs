using EventDbLite.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.Node.Esp32.Abstractions;

namespace NaeTime.Hardware.Node.Esp32;
internal class NodeConnection : INodeConnection
{
    private static readonly byte[] allLanes = [0, 1, 2, 3, 4, 5, 6, 7];

    private readonly INodeCommunication _communication;
    private readonly INodeProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IStreamEventWriter _writer;
    private readonly INaeTimeNodeCommandHandler _commandHandler;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private Task[] _runningTasks =[];

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

    }
    public Task Start()
    {

        CancellationToken token = _cancellationTokenSource.Token;
        _runningTasks = [MaintainConnectionAsync(token), WaitForRSSIAsync(token), WaitForPassAsync(token)];
        return Task.CompletedTask;
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
    public ValueTask<bool> SetLaneRadioFrequency(byte Lane,byte? bandId, int frequencyInMhz)
    {
        if (!IsConnected)
        {
            return ValueTask.FromResult(false);
        }

        return _protocol.ConfigurationProtocol.SetLaneFrequency(Lane, bandId,(ushort)frequencyInMhz);
    }

    public ValueTask<bool> SetLaneEntryThreshold(byte Lane, ushort threshold)
    {
        if (!IsConnected)
        {
            return ValueTask.FromResult(false);
        }

        return _protocol.ConfigurationProtocol.SetEntryThreshold(Lane, threshold);
    }

    public ValueTask<bool> SetLaneExitThreshold(byte Lane, ushort threshold)
    {
        if (!IsConnected)
        {
            return ValueTask.FromResult(false);
        }

        return _protocol.ConfigurationProtocol.SetExitThreshold(Lane, threshold);
    }

    public ValueTask<bool> SetLaneEnabled(byte Lane, bool isEnabled)
    {
        if (!IsConnected)
        {
            return ValueTask.FromResult(false);
        }
        return _protocol.ConfigurationProtocol.SetLaneEnabled(Lane, isEnabled);
    }

    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return Task.WhenAll(_runningTasks);
    }

    public async Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetAllLaneConfigurations()
    {
        if(!IsConnected)
        {
            return Enumerable.Empty<NaeTimeNodeLaneConfiguration>();
        }

        return await _protocol.ConfigurationProtocol.GetLaneConfiguration(allLanes).ConfigureAwait(false);

    }

    public async Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfigurations(IEnumerable<byte> lanes)
    {
        if (!IsConnected)
        {
            return Enumerable.Empty<NaeTimeNodeLaneConfiguration>();
        }

        return await _protocol.ConfigurationProtocol.GetLaneConfiguration(lanes).ConfigureAwait(false);
    }

    public Task<IEnumerable<NaeTimeNodeLaneConfiguration>> GetLaneConfigurations(params byte[] lanes) => GetLaneConfigurations((IEnumerable<byte>)lanes);
}
