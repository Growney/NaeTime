using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface IHardwareRepository
{
    public Task AddOrUpdateEthernetLapRF8Channel(Guid id, string name, byte[] ipAddress, int port);
    public Task SetTimerConnectionStatus(Guid id, bool isConnected, DateTime utcTime);

    public Task<IEnumerable<EthernetLapRF8Channel>> GetAllEthernetLapRF8ChannelTimers();
    public Task<EthernetLapRF8Channel?> GetEthernetLapRF8Channel(Guid id);
    public Task<IEnumerable<TimerDetails>> GetAllTimerDetails();
}
