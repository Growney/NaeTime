using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface IOpenPracticeSessionRepository
{
    public Task<OpenPracticeSession?> Get(Guid sessionId);
    public Task AddOrUpdate(Guid sessionId, string name, Guid trackId, long minimumLapMilliseconds, long? maximumLapMilliseconds);
    public Task SetMinimumLap(Guid sessionId, long minimumLapMilliseconds);
    public Task SetMaximumLap(Guid sessionId, long? maximumLapMilliseconds);
    public Task AddLapToSession(Guid sessionId, Guid lapId, Guid pilotId, OpenPracticeLapStatus status, DateTime startedUtc, DateTime finishedUtc, long totalMilliseconds);
    public Task SetSessionLanePilot(Guid sessionId, byte lane, Guid pilotId);
    public Task RemoveLap(Guid sessionId, Guid lapId);
    public Task SetLapStatus(Guid lapId, OpenPracticeLapStatus status);
    public Task AddTrackedConsecutiveLaps(Guid sessionId, uint lapCap);
    public Task RemoveTrackedConsecutiveLaps(Guid sessionId, uint lapCap);
}
