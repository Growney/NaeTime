using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface ILapRecordRepository
{
    Task RemoveConsecutiveLapRecord(Guid sessionId, Guid pilotId, uint lapCap);
    Task AddOrUpdateConsecutiveLapRecord(Guid sessionId, Guid pilotId, uint lapCap, uint totalLaps, long totalMilliseconds, DateTime lastLapCompletionUtc, IEnumerable<Guid> includedLaps);

    Task<IEnumerable<ConsecutiveLapRecord>> GetPilotConsecutiveLapRecords(Guid sessionId, Guid pilotId);
}
