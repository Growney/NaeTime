namespace NaeTime.Persistence.Abstractions;
public interface ITimingRepository
{
    public Task SetLanePilot(byte lane, Guid? pilotId);
    public Task SetLaneRadioFrequency(byte lane, int frequencyInMhz);
    public Task SetLaneStatus(byte lane, bool isEnabled);
    public Task AddTimerDetection(Guid timerId, byte lane, ulong? hardwareTime, long softwareTime, DateTime utcTime);
    public Task AddManualDetection(Guid timerId, byte lane, long softwareTime, DateTime utcTime);
}
