using NaeTime.Persistence.Abstractions.Timing;

namespace NaeTime.Orchestrator.Persistence.Abstractions;

public interface ITimingOrchestratorPersistence
{
    public Task<Guid> AddDetection(Guid sessionId, Guid trackId, Guid timerId, int timerIndex, byte lane, Guid? pilotId, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task<Guid> AddLap(Guid sessionId, Guid pilotId, Guid entryDetection, LapStatus status);
    public Task<Guid> AddLap(Guid sessionId, Guid pilotId, Guid entryDetection, Guid? exitDetection, LapStatus status);
    public Task CompleteLap(Guid lapId, Guid exitDetection, LapStatus status);
    public Task UpdateLap(Guid lapId, Guid entryDetection, Guid? exitDetection, LapStatus status);

    public Task<IEnumerable<Detection>> GetSessionPilotDetections(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds);
    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId);
    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId);
    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId);
}
