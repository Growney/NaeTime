using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface IPilotProjection
{
    IEnumerable<Pilot> GetAllPilots();
    Pilot? GetPilotById(Guid pilotId);
}