using ImmersionRC.LapRF.Protocol;
using NaeTime.Timing.Abstractions;
using NaeTime.Timing.Abstractions.Models;

namespace NaeTime.Timing.ImmersionRC;
internal class LapRFTimer : Abstractions.IDetector
{
    private readonly Guid _timerId;
    private readonly ISoftwareTimer _softwareTimer;
    private readonly IPassingRecordProtocol _passingProtocol;

    public LapRFTimer(Guid timerId, ISoftwareTimer softwareTimer, IPassingRecordProtocol passingProtocol)
    {
        _timerId = timerId;
        _softwareTimer = softwareTimer;
        _passingProtocol = passingProtocol;
    }

    public async Task<Detection?> WaitForNextDetectionAsync(CancellationToken token)
    {
        var passingRecord = await _passingProtocol.WaitForNextPassAsync(token);

        if (passingRecord == null)
        {
            return null;
        }

        var lapRFPass = passingRecord.Value;

        return new Detection(Guid.NewGuid(), lapRFPass.RealTimeClockTime, _softwareTimer.ElapsedMilliseconds, DateTime.UtcNow, _timerId, lapRFPass.PilotId);

    }
}
