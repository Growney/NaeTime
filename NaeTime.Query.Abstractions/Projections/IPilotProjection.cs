using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface IPilotProjection
{
    IEnumerable<Pilot> GetAllPilots();
    Pilot? GetPilotById(Guid pilotId);
    byte[]? GetPilotBindingPhrase(Guid pilotId);
}