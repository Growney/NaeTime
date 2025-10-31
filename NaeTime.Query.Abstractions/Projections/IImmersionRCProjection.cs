using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IImmersionRCProjection
{
    Ethernet8ChannelImmersionRCLapRF? GetImmersionRCLapRF(Guid id);
    IEnumerable<ImmersionRCLapRF> GetAllImmersionRCLapRFs();
}