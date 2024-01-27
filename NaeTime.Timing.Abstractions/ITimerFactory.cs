namespace NaeTime.Timing.Abstractions;
public interface ITimerFactory
{
    public Task<IDetector> CreateTimers();
}
