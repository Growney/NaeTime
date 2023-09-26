using System.Collections.Concurrent;
using System.Diagnostics;

namespace ImmersionRC.LapRF.Protocol;
internal class StatusProtocol : IStatusProtocol, IDisposable
{
    private readonly AwaitableQueue<ReceivedSignalStrengthIndicator?> _receivedSignalStrengthIndicators = new(5000);
    private readonly ConcurrentDictionary<byte, ReceivedSignalStrengthIndicator> _lastReceivedSignalStrengthIndicators = new();

    private readonly Stopwatch _stopwatch = new();
    private Status? _lastStatus;
    private TimeSpan? _lastStatusTimestamp = null;
    public Status? CurrentStatus => _lastStatus;
    public TimeSpan? CurrentStatusAge => _lastStatusTimestamp == null ? null : _stopwatch.Elapsed - _lastStatusTimestamp;

    public StatusProtocol()
    {
        _stopwatch.Start();
    }

    public void HandleRecordData(ReadOnlySpanReader<byte> recordReader)
    {
        byte? currentTransponderId = null;
        ulong? currentTimestamp = null;

        ushort? inputVoltage = null;
        byte? gateState = null;
        ushort? statusFlags = null;
        uint? statusCount = null;


        while (recordReader.HasData())
        {
            var fieldSignature = recordReader.ReadByte();

            if (fieldSignature == LapRFProtocol.END_OF_RECORD)
            {
                break;
            }

            ///field size read this to clear off the buffer but i am not going to check it because i don't think it makes a difference if it does then i will have to change this
            _ = recordReader.ReadByte();

            switch ((StatusField)fieldSignature)
            {
                case StatusField.TransponderId:
                    currentTransponderId = recordReader.ReadByte();
                    break;
                case StatusField.Timestamp:
                    currentTimestamp = recordReader.ReadUInt64();
                    break;
                case StatusField.StatusFlags:
                    statusFlags = recordReader.ReadUInt16();
                    break;
                case StatusField.InputVoltage:
                    inputVoltage = recordReader.ReadUInt16();
                    break;
                case StatusField.ReceivedSignalStrengthIndicator:
                    var receivedSignalStrengthIndicator = recordReader.ReadSingle();
                    if (currentTransponderId != null)
                    {
                        AddReceivedSignalStrengthIndicator(new ReceivedSignalStrengthIndicator(currentTransponderId.Value, receivedSignalStrengthIndicator, currentTimestamp));
                        currentTransponderId = null;
                    }
                    break;
                case StatusField.GateState:
                    gateState = recordReader.ReadByte();
                    break;
                case StatusField.StatusCount:
                    statusCount = recordReader.ReadUInt32();
                    break;
                default:
                    break;
            }
        }

        if (inputVoltage != null || gateState != null || statusFlags != null || statusCount != null)
        {
            _lastStatus = new Status(inputVoltage, gateState, statusFlags, statusCount);
            _lastStatusTimestamp = _stopwatch.Elapsed;
        }

    }
    private void AddReceivedSignalStrengthIndicator(ReceivedSignalStrengthIndicator receivedSignalStrengthIndicator)
    {
        _lastReceivedSignalStrengthIndicators.AddOrUpdate(receivedSignalStrengthIndicator.TransponderId
            , receivedSignalStrengthIndicator, (key, oldValue) => receivedSignalStrengthIndicator);
        _receivedSignalStrengthIndicators.Enqueue(receivedSignalStrengthIndicator);
    }
    public ReceivedSignalStrengthIndicator? GetLastReceivedSignalStrengthIndicator(byte transponderId)
    {
        if (_lastReceivedSignalStrengthIndicators.TryGetValue(transponderId, out var receivedSignalStrengthIndicator))
        {
            return receivedSignalStrengthIndicator;
        }
        return null;
    }

    public Task<ReceivedSignalStrengthIndicator?> WaitForNextReceivedSignalStrengthIndicatorAsync(CancellationToken cancellationToken)
    {
        return _receivedSignalStrengthIndicators.WaitForDequeueAsync(cancellationToken);
    }

    public void Dispose()
    {
        _receivedSignalStrengthIndicators.Dispose();
        _stopwatch.Stop();
    }
}
