namespace NaeTime.Persistence.Abstractions;
public interface ITimingRepository
{
    public Task AddTimerDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task AddManualDetection(Guid timerId, byte lane, long softwareTime, DateTime utcTime);
}
