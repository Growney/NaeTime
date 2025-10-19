using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface IImmersionRCProjection
{
    Ethernet8ChannelImmersionRCLapRF? GetImmersionRCLapRF(Guid id);
}