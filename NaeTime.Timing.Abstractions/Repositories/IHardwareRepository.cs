using NaeTime.Timing.Abstractions.Models;

namespace NaeTime.Timing.Abstractions.Repositories;
public interface IHardwareRepository
{
    Task<IEnumerable<EthernetLapRF8Channel>> GetAllEthernetLapRF8ChannelAsync();
}
