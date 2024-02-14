using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface IActiveRepository
{
    public Task<ActiveTimings> GetTimings(Guid sessionId, byte lane);
    public Task<IEnumerable<ActiveTimings>> GetTimings(Guid sessionId);
    public Task<ActiveSession?> GetSession();
    public Task ActivateSession(Guid sessionId, SessionType type);
    public Task DeactivateSession();
    public Task ActivateLap(Guid sessionId, byte lane, uint lapNumber, long startedSoftwareTime, DateTime startedUtcTime, ulong? startedHardwareTime);
    public Task DeactivateLap(Guid sessionId, byte lane);
    public Task ActivateSplit(Guid sessionId, byte lane, uint lapNumber, byte splitNumber, long startedSoftwareTime, DateTime startedUtcTime);
    public Task DeactivateSplit(Guid sessionId, byte lane);

}
