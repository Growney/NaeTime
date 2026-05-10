using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IInterfaceProjection
{
    Interface? GetInterface(Guid id);
    IEnumerable<Interface> GetInterfaces();
    IEnumerable<Interface> GetInterfaces(IEnumerable<Guid> ids);
}
