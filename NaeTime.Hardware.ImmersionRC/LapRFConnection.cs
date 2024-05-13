using ImmersionRC.LapRF;
using ImmersionRC.LapRF.Abstractions;
using NaeTime.Hardware.Abstractions;
using NaeTime.Hardware.ImmersionRC.Models;
using NaeTime.Hardware.Messages.Messages;
using NaeTime.PubSub.Abstractions;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFConnection
{
    private readonly ILapRFCommunication _communication;
    private readonly ILapRFProtocol _protocol;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IEventClient _eventClient;
    private readonly Guid _timerId;

    private readonly CancellationTokenSource _cancellationTokenSource;
    public bool IsConnected { get; private set; }

    private readonly Task[] _runningTasks;

    public LapRFConnection(Guid timerId, ISoftwareTimer softwareTimer, IEventClient eventClient, ILapRFCommunication communication, ILapRFProtocol protocol)
    {
        _timerId = timerId;
        _eventClient = eventClient;
        _softwareTimer = softwareTimer ?? throw new ArgumentNullException(nameof(softwareTimer));
        _communication = communication ?? throw new ArgumentNullException(nameof(communication));
        _protocol = protocol ?? throw new ArgumentNullException(nameof(protocol));

        _cancellationTokenSource = new CancellationTokenSource();

        CancellationToken token = _cancellationTokenSource.Token;

        _runningTasks = [MaintainConnectionAsync(token), WaitForDetectionsAsync(token), WaitForStatusAsync(token)];
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

                await _eventClient.Publish(new TimerConnectionEstablished(_timerId, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);

                await runTask;
            }
            catch
            {
                await Task.Delay(1000).ConfigureAwait(false);
            }
            if (IsConnected)
            {
                IsConnected = false;
                await _eventClient.Publish(new TimerDisconnected(_timerId, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow)).ConfigureAwait(false);
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

                TimerDetectionOccured detection = new(_timerId, passingRecord.PilotId, passingRecord.RealTimeClockTime / 1000, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow);

                await _eventClient.Publish(detection).ConfigureAwait(false);
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

                RssiLevelRecorded level = new(_timerId, status.TransponderId, status.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, status.Level);

                await _eventClient.Publish(level).ConfigureAwait(false);
            }
            catch
            {

            }
        }
    }
    public async Task<IEnumerable<LapRF8ChannelLaneConfiguration>> GetLaneConfigurations(IEnumerable<byte> lanes)
    {
        if (!IsConnected)
        {
            return Enumerable.Empty<LapRF8ChannelLaneConfiguration>();
        }

        IEnumerable<RFSetup> rfSetups = await _protocol.RadioFrequencySetupProtocol.GetSetupAsync(lanes, CancellationToken.None).ConfigureAwait(false);

        List<LapRF8ChannelLaneConfiguration> channels = new();

        foreach (RFSetup setup in rfSetups)
        {
            if (setup.Frequency == null)
            {
                continue;
            }
            channels.Add(new LapRF8ChannelLaneConfiguration(setup.TransponderId, null, (int)setup.Frequency.Value, setup.IsEnabled));
        }

        return channels;
    }
    public Task<IEnumerable<LapRF8ChannelLaneConfiguration>> GetLaneConfigurations(params byte[] lanes) => GetLaneConfigurations(lanes.AsEnumerable<byte>());
    public Task<IEnumerable<LapRF8ChannelLaneConfiguration>> GetAllLaneConfigurations() => GetLaneConfigurations([1, 2, 3, 4, 5, 6, 7, 8]);
    public async Task SetLaneStatus(byte Lane, bool isEnabled)
    {
        if (!IsConnected)
        {
            return;
        }
        await _protocol.RadioFrequencySetupProtocol.SetupTransponderSlot(Lane, isEnabled: isEnabled).ConfigureAwait(false);
    }
    public async Task SetLaneRadioFrequency(byte Lane, int frequencyInMhz)
    {
        if (!IsConnected)
        {
            return;
        }
        await _protocol.RadioFrequencySetupProtocol.SetupTransponderSlot(Lane, frequencyInMHz: (ushort)frequencyInMhz).ConfigureAwait(false);
    }
    public Task Stop()
    {
        _cancellationTokenSource.Cancel();

        return Task.WhenAll(_runningTasks);
    }
}
