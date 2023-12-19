using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IPilotApiClient
{
    Task<IEnumerable<Pilot>> GetAllAsync();
    Task<Pilot?> CreateAsync(string? firstname, string? lastname, string? callsign);
    Task<Pilot?> UpdateAsync(Pilot update);
    Task<Pilot?> GetAsync(Guid id);
}
