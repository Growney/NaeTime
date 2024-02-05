using NaeTime.Messages.Events.Timing;
using NaeTime.Messages.Requests;
using NaeTime.Messages.Responses;
using NaeTime.PubSub;
using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;
using NaeTime.Timing.Models;

namespace NaeTime.Timing;
public class LapManager : ISubscriber
{
    private readonly IPublishSubscribe _publishSubscribe;

    public LapManager(IPublishSubscribe publishSubscribe)
    {
        _publishSubscribe = publishSubscribe ?? throw new ArgumentNullException(nameof(publishSubscribe));
    }
    public Task When(TimerDetectionOccured detection) => HandleDetection(detection.TimerId, detection.Lane, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
    public async Task When(TimerDetectionTriggered detection) => await HandleDetection(detection.TimerId, detection.Lane, null, detection.SoftwareTime, detection.UtcTime);

    private async Task HandleDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var activeTrackResponse = await _publishSubscribe.Request<ActiveTrackRequest, ActiveTrackResponse>();

        if (activeTrackResponse == null)
        {
            return;
        }

        ActiveTrack activeTrack = new ActiveTrack(activeTrackResponse.TrackId, activeTrackResponse.MinimumLapMilliseconds, activeTrackResponse.MaximumLapMilliseconds, activeTrackResponse.Timers);

        var activeTimingsResponse = await _publishSubscribe.Request<ActiveTimingRequest, ActiveTimingsResponse>(new ActiveTimingRequest(lane));

        ActiveLap? activeLap = null;
        if (activeTimingsResponse?.Lap != null)
        {
            activeLap = new ActiveLap(activeTimingsResponse.Lap.LapNumber, activeTimingsResponse.Lap.StartedSoftwareTime, activeTimingsResponse.Lap.StartedUtcTime, activeTimingsResponse.Lap.StartedHardwareTime);
        }

        ActiveSplit? activeSplit = null;
        if (activeTimingsResponse?.Split != null)
        {
            activeSplit = new ActiveSplit(activeTimingsResponse.Split.LapNumber, activeTimingsResponse.Split.SplitNumber, activeTimingsResponse.Split.StartedSoftwareTime, activeTimingsResponse.Split.StartedUtcTime);
        }

        var timerPosition = activeTrack.GetTimerPosition(timerId);
        //Timer is not on the track or their are too many timers for some reason
        if (timerPosition == -1 || timerPosition > byte.MaxValue)
        {
            return;
        }

        var timerCount = activeTrack.Timers.Count();
        //No timers WHAT !? or too many timers
        if (timerCount == 0 || timerCount > byte.MaxValue)
        {
            return;
        }

        //there is nothing active we should start things
        if (activeLap == null && activeSplit == null)
        {
            if (timerPosition == 0)
            {
                await StartLap(activeTrack.Id, lane, 0, hardwareTime, softwareTime, utcTime);
            }


            if (timerCount > 0)
            {
                await StartSplit(activeTrack.Id, 0, lane, softwareTime, utcTime, timerPosition);
            }
        }
        else
        {
            uint lapNumber;
            if (activeLap == null)
            {
                await StartLap(activeTrack.Id, lane, 0, hardwareTime, softwareTime, utcTime);
                lapNumber = 0;
            }
            else
            {
                lapNumber = await HandleLapDetection(activeTrack.Id, lane, hardwareTime, softwareTime, utcTime, activeLap, activeTrack.MinimumLapMilliseconds, activeTrack.MaximumLapMilliseconds);
            }
            if (timerCount > 0)
            {
                await HandleSplitDetection(activeTrack.Id, lane, softwareTime, utcTime, activeLap, activeSplit, (byte)timerPosition, (byte)timerCount, lapNumber);
            }
        }
    }

    private async Task<uint> HandleLapDetection(Guid trackId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime, ActiveLap activeLap, long minimumLapMilliseconds, long maximumLapMilliseconds)
    {
        var totalTime = CalculateTotalTime(activeLap.StartedHardwareTime, activeLap.StartedSoftwareTime, activeLap.StartedUtcTime, hardwareTime, softwareTime, utcTime);
        if (totalTime < minimumLapMilliseconds)
        {
            await InvalidateLap(trackId, lane, hardwareTime, softwareTime, utcTime, activeLap, totalTime, LapInvalidReason.TooShort);
        }
        else if (totalTime > maximumLapMilliseconds)
        {
            await InvalidateLap(trackId, lane, hardwareTime, softwareTime, utcTime, activeLap, totalTime, LapInvalidReason.TooLong);
        }
        else
        {
            await CompleteLap(trackId, lane, hardwareTime, softwareTime, utcTime, activeLap, totalTime);
        }

        var nextLapNumber = activeLap.LapNumber + 1;

        await StartLap(trackId, lane, nextLapNumber, hardwareTime, softwareTime, utcTime);

        return nextLapNumber;
    }
    private async Task InvalidateLap(Guid trackId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime, ActiveLap activeLap, long totalTime, LapInvalidReason reason)
    {
        var completedLap = new LapInvalidated(trackId, lane, activeLap.LapNumber, softwareTime, utcTime, hardwareTime, totalTime, reason switch
        {
            LapInvalidReason.TooShort => LapInvalidated.LapInvalidReason.TooShort,
            LapInvalidReason.TooLong => LapInvalidated.LapInvalidReason.TooLong,
            _ => throw new NotImplementedException()
        });

        await _publishSubscribe.Dispatch(completedLap);
    }
    private async Task CompleteLap(Guid trackId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime, ActiveLap activeLap, long totalTime)
    {
        var completedLap = new LapCompleted(trackId, lane, activeLap.LapNumber, softwareTime, utcTime, hardwareTime, totalTime);

        await _publishSubscribe.Dispatch(completedLap);
    }
    private long CalculateTotalTime(ulong? startHardwareTime, long startSoftwareTime, DateTime startUtcTime, ulong? endHardwareTime, long endSoftwareTime, DateTime endUtcTime)
    {
        if (startHardwareTime.HasValue && endHardwareTime.HasValue)
        {
            if (endHardwareTime.Value > startHardwareTime.Value)
            {
                return (long)(endHardwareTime.Value - startHardwareTime.Value);
            }
        }

        var softwareDifference = endSoftwareTime - startSoftwareTime;
        if (softwareDifference < 0)
        {
            return (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds;
        }
        else
        {
            return softwareDifference;
        }
    }
    private IEnumerable<(uint lapNumber, byte splitNumber)> GetMissingSplits(byte timerCount, uint activeLap, byte activeSplit, uint detectedLap, byte detectedSplit)
    {
        byte startSplit = activeSplit;
        byte maxSplit = timerCount;

        for (var lapIndex = activeLap; lapIndex <= detectedLap; lapIndex++)
        {
            if (lapIndex == detectedLap)
            {
                maxSplit = detectedSplit;
            }

            for (var splitIndex = startSplit; splitIndex < maxSplit; splitIndex++)
            {
                yield return (lapIndex, splitIndex);
            }
            startSplit = 0;
        }
    }
    private async Task HandleSplitDetection(Guid trackId, byte lane, long softwareTime, DateTime utcTime, ActiveLap? activeLap, ActiveSplit? activeSplit, byte timerPosition, byte timerCount, uint lapNumber)
    {
        if (activeSplit == null)
        {
            await StartSplit(trackId, lapNumber, lane, softwareTime, utcTime, timerPosition);
        }
        else
        {
            var currentSplitNumber = activeSplit.SplitNumber;

            int expectedTimerPosition;
            if (currentSplitNumber == timerCount - 1)
            {
                expectedTimerPosition = 0;
            }
            else
            {
                expectedTimerPosition = currentSplitNumber + 1;
            }

            uint expectedLapNumber;
            //If we are the last split we expect the next lap on the next detection
            if (timerPosition == timerCount - 1)
            {
                expectedLapNumber = activeSplit.LapNumber + 1;
            }
            else
            {
                expectedLapNumber = activeSplit.LapNumber;
            }


            if (expectedLapNumber != lapNumber || expectedTimerPosition != timerPosition)
            {
                await HandleSkippedSplits(trackId, lane, timerCount, activeLap?.LapNumber ?? 0, currentSplitNumber, lapNumber, timerPosition);
            }

            var totalTime = CalculateTotalTime(activeSplit.StartedSoftwareTime, activeSplit.StartedUtcTime, softwareTime, utcTime);

            await CompleteSplit(trackId, lane, softwareTime, utcTime, lapNumber, currentSplitNumber, totalTime);

            await StartSplit(trackId, lapNumber, lane, softwareTime, utcTime, timerPosition);

        }
    }
    private async Task CompleteSplit(Guid trackId, byte lane, long softwareTime, DateTime utcTime, uint lapNumber, byte currentSplitNumber, long totalTime)
    {
        var splitEnded = new SplitCompleted(trackId, lane, lapNumber, (byte)currentSplitNumber, softwareTime, utcTime, totalTime);

        await _publishSubscribe.Dispatch(splitEnded);
    }
    private long CalculateTotalTime(long startSoftwareTime, DateTime startUtcTime, long endSoftwareTime, DateTime endUtcTime)
    {
        var softwareDifference = endSoftwareTime - startSoftwareTime;
        if (softwareDifference < 0)
        {
            return (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds;
        }
        else
        {
            return softwareDifference;
        }
    }
    private async Task HandleSkippedSplits(Guid trackId, byte lane, byte timerCount, uint activeLap, byte activeSplit, uint detectedLap, byte detectedSplit)
    {
        foreach (var missingSplit in GetMissingSplits(timerCount, activeLap, activeSplit, detectedLap, detectedSplit))
        {
            var skippedSplit = new SplitSkipped(trackId, lane, missingSplit.lapNumber, missingSplit.splitNumber);

            await _publishSubscribe.Dispatch(skippedSplit);
        }
    }
    private async Task StartSplit(Guid trackId, uint lapNumber, byte lane, long softwareTime, DateTime utcTime, int timerPosition)
    {
        var splitStarted = new SplitStarted(trackId, lane, lapNumber, (byte)timerPosition, softwareTime, utcTime);

        await _publishSubscribe.Dispatch(splitStarted);
    }
    private async Task StartLap(Guid trackId, byte lane, uint lapNumber, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var lapStarted = new LapStarted(trackId, lane, lapNumber, softwareTime, utcTime, hardwareTime);

        await _publishSubscribe.Dispatch(lapStarted);
    }
}
