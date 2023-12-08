using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IPilotApiClient
{
    Task<IEnumerable<Pilot>> GetAllPilotsAsync();
    Task<Pilot?> CreatePilotAsync(string? firstname, string? lastname, string? callsign);
    Task<Pilot?> GetPilotDetailsAsync(Guid id);
}
