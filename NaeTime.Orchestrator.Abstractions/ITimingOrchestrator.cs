using NaeTime.Persistence.Abstractions.Timing;

namespace NaeTime.Orchestrator.Abstractions;

public interface ITimingOrchestrator
{
    public Task AddSessionDetection(Guid sessionId, Guid track, Guid timerId, Guid? pilotId, int timerIndex, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);

    public Task<IEnumerable<Lap>> GetPilotOpenPracticeSessionLaps(Guid sessionId, Guid pilotId);
    public Task<IEnumerable<Lap>> GetOpenPracticeSessionLaps(Guid sessionId);
    public Task<Lap?> GetOpenPracticeSessionLap(Guid lapId);
    public Task<IEnumerable<Lap>> GetOpenPracticeLaps(IEnumerable<Guid> lapIds);
}
