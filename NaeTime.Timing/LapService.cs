using NaeTime.PubSub.Abstractions;
using NaeTime.Timing.Messages.Events;
using NaeTime.Timing.Models;

namespace NaeTime.Timing;
internal class LapService
{
    private readonly IEventClient _eventClient;
    private readonly IRemoteProcedureCallClient _rpcClient;

    public LapService(IEventClient eventClient, IRemoteProcedureCallClient rpcClient)
    {
        _eventClient = eventClient ?? throw new ArgumentNullException(nameof(eventClient));
        _rpcClient = rpcClient ?? throw new ArgumentNullException(nameof(rpcClient));
    }

    public async Task When(SessionDetectionOccured detection)
    {
        var track = await _rpcClient.InvokeAsync<Management.Messages.Models.Track?>("GetTrack", detection.TrackId);

        if (track == null)
        {
            return;
        }

        var activeTrack = new ActiveTrack(track.Timers);
        var timerPosition = detection.TimerId == Guid.Empty ? 0 : activeTrack.GetTimerPosition(detection.TimerId);
        var timerCount = activeTrack.Timers.Count();

        if (timerPosition < 0 || timerPosition > byte.MaxValue || timerCount > byte.MaxValue)
        {
            //TODO dispatch timer detection discarded
            return;
        }
        await HandleDetection(detection.SessionId, detection.MinimumLapMilliseconds, detection.MaximumLapMilliseconds, detection.Lane, (byte)timerPosition, (byte)timerCount, detection.HardwareTime, detection.SoftwareTime, detection.UtcTime);
    }


    private async Task HandleDetection(Guid sessionId, long minimumLapMilliseconds, long? maximumLapMilliseconds, byte lane, byte split, byte timerCount, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var activeTimingsResponse = await _rpcClient.InvokeAsync<Messages.Models.LaneActiveTimings>("GetLaneActiveTimings").ConfigureAwait(false);

        ActiveLap? activeLap = null;
        if (activeTimingsResponse?.Lap != null)
        {
            activeLap = new ActiveLap(activeTimingsResponse.LapNumber, activeTimingsResponse.Lap.StartedSoftwareTime, activeTimingsResponse.Lap.StartedUtcTime, activeTimingsResponse.Lap.StartedHardwareTime);
        }

        ActiveSplit? activeSplit = null;
        if (activeTimingsResponse?.Split != null)
        {
            activeSplit = new ActiveSplit(activeTimingsResponse.LapNumber, activeTimingsResponse.Split.SplitNumber, activeTimingsResponse.Split.StartedSoftwareTime, activeTimingsResponse.Split.StartedUtcTime);
        }

        //there is nothing active we should start things
        if (activeLap == null && activeSplit == null)
        {
            if (split == 0)
            {
                await StartLap(sessionId, lane, 0, hardwareTime, softwareTime, utcTime).ConfigureAwait(false);
            }

            if (timerCount > 0)
            {
                await StartSplit(sessionId, 0, lane, softwareTime, utcTime, split).ConfigureAwait(false);
            }
        }
        else
        {
            uint lapNumber;
            if (activeLap == null)
            {
                await StartLap(sessionId, lane, 0, hardwareTime, softwareTime, utcTime).ConfigureAwait(false);
                lapNumber = 0;
            }
            else
            {
                lapNumber = await HandleLapDetection(sessionId, lane, hardwareTime, softwareTime, utcTime, split, activeLap, minimumLapMilliseconds, maximumLapMilliseconds).ConfigureAwait(false);
            }

            if (timerCount > 0)
            {
                await HandleSplitDetection(sessionId, lane, softwareTime, utcTime, activeLap, activeSplit, split, timerCount, lapNumber).ConfigureAwait(false);
            }
        }
    }
    private async Task<uint> HandleLapDetection(Guid sessionId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime, byte split, ActiveLap activeLap, long minimumLapMilliseconds, long? maximumLapMilliseconds)
    {
        if (split != 0)
        {
            return activeLap.LapNumber;
        }

        var totalTime = CalculateTotalTime(activeLap.StartedHardwareTime, activeLap.StartedSoftwareTime, activeLap.StartedUtcTime, hardwareTime, softwareTime, utcTime);
        //Discard the detection as the lap is too short
        if (totalTime < minimumLapMilliseconds)
        {
            return activeLap.LapNumber;
        }

        //Laps that are two long get invalidated instead of the detection being discarded
        if (maximumLapMilliseconds.HasValue && totalTime > maximumLapMilliseconds)
        {
            await InvalidateLap(sessionId, lane, activeLap.StartedHardwareTime, activeLap.StartedSoftwareTime, activeLap.StartedUtcTime, hardwareTime, softwareTime, utcTime, activeLap, totalTime, LapInvalidReason.TooLong).ConfigureAwait(false);
        }
        else
        {
            await CompleteLap(sessionId, lane, activeLap.StartedHardwareTime, activeLap.StartedSoftwareTime, activeLap.StartedUtcTime, hardwareTime, softwareTime, utcTime, activeLap, totalTime).ConfigureAwait(false);
        }

        var nextLapNumber = activeLap.LapNumber + 1;

        await StartLap(sessionId, lane, nextLapNumber, hardwareTime, softwareTime, utcTime).ConfigureAwait(false);

        return nextLapNumber;
    }
    private async Task InvalidateLap(Guid sessionId, byte lane, ulong? startedHardwareTime, long startedSoftwareTime, DateTime startedUtcTime, ulong? finishedHardwareTime, long finishedSoftwareTime, DateTime finishedUtcTime, ActiveLap activeLap, long totalTime, LapInvalidReason reason)
    {
        var completedLap = new LapInvalidated(sessionId, lane, activeLap.LapNumber, startedSoftwareTime, startedUtcTime, startedHardwareTime, finishedSoftwareTime, finishedUtcTime, finishedHardwareTime, totalTime, reason switch
        {
            LapInvalidReason.TooShort => LapInvalidated.LapInvalidReason.TooShort,
            LapInvalidReason.TooLong => LapInvalidated.LapInvalidReason.TooLong,
            _ => throw new NotImplementedException()
        });

        await _eventClient.Publish(completedLap).ConfigureAwait(false);
    }
    private async Task CompleteLap(Guid sessionId, byte lane, ulong? startedHardwareTime, long startedSoftwareTime, DateTime startedUtcTime, ulong? finishedHardwareTime, long finishedSoftwareTime, DateTime finishedUtcTime, ActiveLap activeLap, long totalTime)
    {
        var completedLap = new LapCompleted(sessionId, lane, activeLap.LapNumber, startedSoftwareTime, startedUtcTime, startedHardwareTime, finishedSoftwareTime, finishedUtcTime, finishedHardwareTime, totalTime);

        await _eventClient.Publish(completedLap).ConfigureAwait(false);
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
        return softwareDifference < 0 ? (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds : softwareDifference;
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
    private async Task HandleSplitDetection(Guid sessionId, byte lane, long softwareTime, DateTime utcTime, ActiveLap? activeLap, ActiveSplit? activeSplit, byte timerPosition, byte timerCount, uint lapNumber)
    {
        if (activeSplit == null)
        {
            await StartSplit(sessionId, lapNumber, lane, softwareTime, utcTime, timerPosition).ConfigureAwait(false);
        }
        else
        {
            var currentSplitNumber = activeSplit.SplitNumber;

            int expectedTimerPosition = currentSplitNumber == timerCount - 1 ? 0 : currentSplitNumber + 1;

            uint expectedLapNumber = timerPosition == timerCount - 1 ? activeSplit.LapNumber + 1 : activeSplit.LapNumber;
            //If we are the last split we expect the next lap on the next detection

            if (expectedLapNumber != lapNumber || expectedTimerPosition != timerPosition)
            {
                await HandleSkippedSplits(sessionId, lane, timerCount, activeLap?.LapNumber ?? 0, currentSplitNumber, lapNumber, timerPosition).ConfigureAwait(false);
            }

            var totalTime = CalculateTotalTime(activeSplit.StartedSoftwareTime, activeSplit.StartedUtcTime, softwareTime, utcTime);

            await CompleteSplit(sessionId, lane, softwareTime, utcTime, lapNumber, currentSplitNumber, totalTime).ConfigureAwait(false);

            await StartSplit(sessionId, lapNumber, lane, softwareTime, utcTime, timerPosition).ConfigureAwait(false);

        }
    }
    private async Task CompleteSplit(Guid sessionId, byte lane, long softwareTime, DateTime utcTime, uint lapNumber, byte currentSplitNumber, long totalTime)
    {
        var splitEnded = new SplitCompleted(sessionId, lane, lapNumber, (byte)currentSplitNumber, softwareTime, utcTime, totalTime);

        await _eventClient.Publish(splitEnded).ConfigureAwait(false);
    }
    private long CalculateTotalTime(long startSoftwareTime, DateTime startUtcTime, long endSoftwareTime, DateTime endUtcTime)
    {
        var softwareDifference = endSoftwareTime - startSoftwareTime;
        return softwareDifference < 0 ? (long)endUtcTime.Subtract(startUtcTime).TotalMilliseconds : softwareDifference;
    }
    private async Task HandleSkippedSplits(Guid sessionId, byte lane, byte timerCount, uint activeLap, byte activeSplit, uint detectedLap, byte detectedSplit)
    {
        foreach (var (lapNumber, splitNumber) in GetMissingSplits(timerCount, activeLap, activeSplit, detectedLap, detectedSplit))
        {
            var skippedSplit = new SplitSkipped(sessionId, lane, lapNumber, splitNumber);

            await _eventClient.Publish(skippedSplit).ConfigureAwait(false);
        }
    }
    private async Task StartSplit(Guid sessionId, uint lapNumber, byte lane, long softwareTime, DateTime utcTime, int timerPosition)
    {
        var splitStarted = new SplitStarted(sessionId, lane, lapNumber, (byte)timerPosition, softwareTime, utcTime);

        await _eventClient.Publish(splitStarted).ConfigureAwait(false);
    }
    private async Task StartLap(Guid sessionId, byte lane, uint lapNumber, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        var lapStarted = new LapStarted(sessionId, lane, lapNumber, softwareTime, utcTime, hardwareTime);

        await _eventClient.Publish(lapStarted).ConfigureAwait(false);
    }
}
