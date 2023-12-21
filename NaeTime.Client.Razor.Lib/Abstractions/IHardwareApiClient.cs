using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IHardwareApiClient
{
    Task<IEnumerable<TimerDetails>> GetAllTimerDetailsAsync();
    Task<EthernetLapRF8Channel?> CreateEthernetLapRF8ChannelAsync(string name, string ipAddress, int port);
    Task<EthernetLapRF8Channel?> GetEthernetLapRF8ChannelDetailsAsync(Guid id);
    Task UpdateEthernetLapRF8ChannelAsync(EthernetLapRF8Channel timer);
}
