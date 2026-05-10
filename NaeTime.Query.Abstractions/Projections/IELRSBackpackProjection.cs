using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IELRSBackpackProjection
{
    SerialELRSBackpackInterface? GetSerialELRSBackpackInterface(Guid id);
    IEnumerable<SerialELRSBackpackInterface> GetAllSerialELRSBackpackInterfaces();
}
