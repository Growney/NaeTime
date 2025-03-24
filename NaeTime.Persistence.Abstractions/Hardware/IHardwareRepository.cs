namespace NaeTime.Persistence.Abstractions.Hardware;

public interface IHardwareRepository
{
    public Task<SerialEsp32Node?> GetSerialEsp32NodeTimer(Guid timerId);
    public Task<EthernetLapRF8ChannelTimer?> GetEthernetLapRF8ChannelTimer(Guid timerId);
    public Task<IEnumerable<EthernetLapRF8ChannelTimer>> GetAllEthernetLapRF8ChannelTimers();
    public Task<IEnumerable<SerialEsp32Node>> GetAllSerialEsp32NodeTimers();
    public Task<IEnumerable<TimerDetails>> GetAllTimerDetails();
    public Task<TimerType> GetTimerType(Guid timerId);
}
