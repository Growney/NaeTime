using NaeTime.Orchestrator.Abstractions;
using NaeTime.Orchestrator.Distribution.Abstractions;
using NaeTime.Orchestrator.Persistence.Abstractions;
using NaeTime.Persistence.Abstractions.Timing;
using NaeTime.Persistence.Abstractions.Timing.Extensions;

namespace NaeTime.Orchestrator;

public class TimingOrchestrator : ITimingOrchestrator
{
    private readonly ITimingOrchestratorPersistence _orchestratorPersistence;
    private readonly INaeTimeOrchestratorDistribution _distribution;

    public TimingOrchestrator(ITimingOrchestratorPersistence persistence, INaeTimeOrchestratorDistribution distribution)
    {
        _orchestratorPersistence = persistence ?? throw new ArgumentNullException(nameof(persistence));
        _distribution = distribution ?? throw new ArgumentNullException(nameof(distribution));
    }

    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId) =>
        _orchestratorPersistence.GetPilotOpenPracticeSessionLaps(sessionId, pilotId);

    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId) =>
        _orchestratorPersistence.GetOpenPracticeSessionLaps(sessionId);

    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId) =>
        _orchestratorPersistence.GetOpenPracticeSessionLap(lapId);

    public Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds) =>
        _orchestratorPersistence.GetOpenPracticeLaps(lapIds);

    public async Task AddSessionDetection(Guid sessionId, Guid trackId, Guid timerId, Guid? pilotId, int timerIndex, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        if (pilotId != null)
        {
            await AddAssignedDetection(sessionId, trackId, timerId, pilotId.Value, timerIndex, lane, hardwareTime, softwareTime, utcTime);
        }
        else
        {
            await AddUnassignedDetection(sessionId, trackId, timerId, timerIndex, lane, hardwareTime, softwareTime, utcTime);
        }
    }
    private Task AddUnassignedDetection(Guid sessionId, Guid trackId, Guid timerId, int timerIndex, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
        => _orchestratorPersistence.AddDetection(sessionId, trackId, timerId, timerIndex, lane, null, hardwareTime, softwareTime, utcTime);


    private async Task AddAssignedDetection(Guid sessionId, Guid trackId, Guid timerId, Guid pilotId, int timerIndex, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime)
    {
        IEnumerable<Detection> pilotDetections = await _orchestratorPersistence.GetSessionPilotDetections(sessionId, pilotId);
        IEnumerable<Lap> pilotLaps = await _orchestratorPersistence.GetPilotOpenPracticeSessionLaps(sessionId, pilotId);

        Dictionary<Guid, Lap> lapsByEntryDetection = pilotLaps.ToDictionary(x => x.EntryDetection.Id);
        Dictionary<Guid, Lap> lapsByExitDetection = pilotLaps.Where(x => x.ExitDetection != null).ToDictionary(x => x.ExitDetection!.Id);

        List<Detection> detectionsInOrder = pilotDetections.ToList();
        detectionsInOrder.Sort(IDetectionExtensions.Compare);

        List<Lap> lapsInOrder = pilotLaps.ToList();
        lapsInOrder.Sort((x, y) => IDetectionExtensions.Compare(x.EntryDetection, y.EntryDetection));

        Detection newDetection = new(Guid.NewGuid(), sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, utcTime);


        await _orchestratorPersistence.AddDetection(sessionId, trackId, timerId, timerIndex, lane, pilotId, hardwareTime, softwareTime, utcTime);
        int newDetectionIndex = 0;

        for (int i = 0; i < detectionsInOrder.Count; i++)
        {
            newDetectionIndex = i;
            int comparison = IDetectionExtensions.Compare(newDetection, detectionsInOrder[i]);
            if (comparison < 0)
            {
                break;
            }
        }

        Detection? previousDetection = detectionsInOrder.Take(newDetectionIndex).LastOrDefault();
        Detection? nextDetection = detectionsInOrder.Skip(newDetectionIndex).FirstOrDefault();

        //laps
        //  has previous |  has next
        //  true         |  false   - complete previous lap, create new lap with entry as new
        //  true         |  true    - change previous lap exit to new, create new lap with new as entry and next as exit
        //  false        |  false   - create new lap with entry as new
        //  false        |  true    - create new lap entry as new, exit as next

        if (previousDetection != null && nextDetection == null)
        {
            Lap? previousLap = lapsByExitDetection.GetValueOrDefault(previousDetection.Id);

            if (previousLap != null)
            {
                await _orchestratorPersistence.CompleteLap(previousLap.Id, newDetection.Id, LapStatus.Valid);
            }

            await _orchestratorPersistence.AddLap(sessionId, pilotId, newDetection.Id, LapStatus.Incomplete);
        }
        else if (previousDetection != null && nextDetection != null)
        {
            Lap? previousLap = lapsByExitDetection.GetValueOrDefault(previousDetection.Id);

            if (previousLap != null)
            {
                await _orchestratorPersistence.UpdateLap(previousLap.Id, previousLap.EntryDetection.Id, newDetection.Id, previousLap.Status);
            }

            Lap? nextLap = lapsByEntryDetection.GetValueOrDefault(nextDetection.Id);

            if (nextLap == null)
            {
                throw new NotImplementedException("unhandled scenario where detection has not created a lap");
            }

            await _orchestratorPersistence.AddLap(sessionId, pilotId, newDetection.Id, nextLap.EntryDetection.Id, LapStatus.Valid);
        }
        else if (previousDetection == null && nextDetection == null)
        {
            await _orchestratorPersistence.AddLap(sessionId, pilotId, newDetection.Id, LapStatus.Incomplete);
        }
        else if (previousDetection == null && nextDetection != null)
        {
            Lap? nextLap = lapsByEntryDetection.GetValueOrDefault(nextDetection.Id);

            if (nextLap == null)
            {
                throw new NotImplementedException("unhandled scenario where detection has not created a lap");
            }

            await _orchestratorPersistence.AddLap(sessionId, pilotId, newDetection.Id, nextLap.EntryDetection.Id, LapStatus.Valid);
        }
        else
        {
            throw new NotImplementedException("Unknown scenario");
        }
    }
}
