namespace NaeTime.Persistence.Abstractions.Hardware;

public interface IHardwareRepository
{
    public Task<SerialEsp32Node?> GetSerialEsp32NodeTimer(Guid timerId);
    public Task<EthernetLapRF8ChannelTimer?> GetEthernetLapRF8ChannelTimer(Guid timerId);
    public Task<IEnumerable<LapRFLaneConfiguration>> GetEthernetLapRF8ChannelTimerLaneConfigurations(Guid timerId);
    public Task<bool> IsTimerConnected(Guid timerId);
    public Task<IEnumerable<TimerDetails>> GetAllTimerDetails();
}
