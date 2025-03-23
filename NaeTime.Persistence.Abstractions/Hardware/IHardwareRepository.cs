namespace NaeTime.Persistence.Abstractions.Hardware;

public interface IHardwareRepository
{
    public Task<IEnumerable<LapRFLaneConfiguration>> GetEthernetLapRF8ChannelTimerLaneConfigurations(Guid timerId);
    public Task<bool> IsTimerConnected(Guid timerId);
}
