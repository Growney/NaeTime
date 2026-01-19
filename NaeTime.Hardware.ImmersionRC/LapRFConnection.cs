using EventDbLite.Abstractions;
using ImmersionRC.LapRF;
using ImmersionRC.LapRF.Abstractions;
using NaeTime.Command.Abstractions;
using NaeTime.Events;
using NaeTime.Hardware;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.ImmersionRC.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnection : ILapRFConnection
{

    private readonly ILapRFCommunication _communication;
    private readonly ILapRFProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IStreamEventWriter _writer;
    private readonly IImmersionRCLapRFCommandHandler _commandHandler;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private Task[] _runningTasks = [];

    private readonly string _detectionsStream;
    private readonly string _rssiStream;

    public LapRFConnection(Guid timerId, ISoftwareTimer softwareTimer, ILapRFCommunication communication, ILapRFProtocol protocol, IStreamEventWriter writer, IImmersionRCLapRFCommandHandler commandHandler)
    {
        _timerId = timerId;

        _detectionsStream = $"ImmersionRC-LapRF-{_timerId}-Detections";
        _rssiStream = $"ImmersionRC-LapRF-{_timerId}-Rssi";

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
        _runningTasks = [MaintainConnectionAsync(token), WaitForDetectionsAsync(token), WaitForStatusAsync(token)];
        return Task.CompletedTask;
    }
    private async Task MaintainConnectionAsync(CancellationToken token)
    {
        await _commandHandler.MarkAsDisconnected(_timerId);
        while (!token.IsCancellationRequested)
        {
            try
            {
                await _communication.ConnectAsync(token).ConfigureAwait(false);
                IsConnected = true;
                await _commandHandler.MarkAsConnected(_timerId);

                //We must start the run task before we dispatch the connection established as data may be requested when the connection is established
                System.Runtime.CompilerServices.ConfiguredTaskAwaitable runTask = _protocol.RunAsync(token).ConfigureAwait(false);


                await runTask;
            }
            catch
            {
                await Task.Delay(500).ConfigureAwait(false);
            }

            if (IsConnected)
            {
                await _commandHandler.MarkAsDisconnected(_timerId);
                IsConnected = false;
            }

            await _communication.DisconnectAsync(token).ConfigureAwait(false);
        }
    }
    private async Task WaitForDetectionsAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                Pass? nullablePassingRecord = await _protocol.PassingRecordProtocol.WaitForNextPassAsync(token).ConfigureAwait(false);

                if (nullablePassingRecord == null)
                {
                    continue;
                }

                Pass passingRecord = nullablePassingRecord.Value;

                await _writer.AppendToStream(_detectionsStream, new NaeTime.Events.HardwareDetectionOccured(Guid.NewGuid(), _timerId, passingRecord.LaneId, passingRecord.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
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
                ReceivedSignalStrengthIndicator? nullableStatus = await _protocol.StatusProtocol.WaitForNextReceivedSignalStrengthIndicatorAsync(token).ConfigureAwait(false);

                if (nullableStatus == null)
                {
                    continue;
                }

                ReceivedSignalStrengthIndicator status = nullableStatus.Value;

                await _writer.AppendToStream(_rssiStream, new RssiRecorded(_timerId, status.LaneId,status.Level,_softwareTimer.ElapsedMilliseconds,status.RealTimeClockTime));

            }
            catch
            {

            }
        }
    }
    public async Task<IEnumerable<LapRFLaneConfiguration>> GetLaneConfigurations(IEnumerable<byte> lanes)
    {
        if (!IsConnected)
        {
            return Enumerable.Empty<LapRFLaneConfiguration>();
        }

        IEnumerable<RFSetup> rfSetups = await _protocol.RadioFrequencySetupProtocol.GetSetupAsync(lanes, CancellationToken.None).ConfigureAwait(false);

        List<LapRFLaneConfiguration> channels = [];

        foreach (RFSetup setup in rfSetups)
        {
            if (setup.Frequency == null)
            {
                continue;
            }

            channels.Add(new LapRFLaneConfiguration(setup.LaneId, (byte)(setup.Band ?? 0), setup.Frequency ?? 0, setup.IsEnabled, setup.Attenuation ?? 0, setup.Threshold ?? 0));
        }

        return channels;
    }
    public Task<IEnumerable<LapRFLaneConfiguration>> GetLaneConfigurations(params byte[] lanes) => GetLaneConfigurations(lanes.AsEnumerable<byte>());
    public Task<IEnumerable<LapRFLaneConfiguration>> GetAllLaneConfigurations() => GetLaneConfigurations([0, 1, 2, 3, 4, 5, 6, 7]);
    public async Task SetLaneStatus(byte Lane, bool isEnabled)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.RadioFrequencySetupProtocol.SetupLane(Lane, isEnabled: isEnabled).ConfigureAwait(false);
    }
    public async Task SetLaneRadioFrequency(byte Lane, byte? bandId, int frequencyInMhz)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.RadioFrequencySetupProtocol.SetupLane(Lane, band: bandId, frequencyInMHz: (ushort)frequencyInMhz).ConfigureAwait(false);
    }
    public async Task SetLaneThreshold(byte lane, float threshold)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.RadioFrequencySetupProtocol.SetupLane(lane, threshold: threshold).ConfigureAwait(false);
    }
    public async Task SetLaneGain(byte lane, ushort gain)
    {
        if (!IsConnected)
        {
            return;
        }

        await _protocol.RadioFrequencySetupProtocol.SetupLane(lane, attenuation: gain).ConfigureAwait(false);
    }
    public async Task SetupLane(LapRFLaneConfiguration configuration)
    {
        if (!IsConnected)
        {
            return;
        }
        await _protocol.RadioFrequencySetupProtocol.SetupLane(configuration.Lane, isEnabled: configuration.IsEnabled, frequencyInMHz: (ushort?)configuration.FrequencyInMhz, attenuation: configuration.Gain, threshold: configuration.Threshold).ConfigureAwait(false);
    }
    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return Task.WhenAll(_runningTasks);
    }
}
