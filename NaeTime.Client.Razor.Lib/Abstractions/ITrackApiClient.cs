using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface ITrackApiClient
{
    Task<IEnumerable<Track>> GetAllAsync();
    Task<Track?> CreateAsync(string name, IEnumerable<TimedGate> gates);
    Task<Track?> UpdateAsync(Track update);
    Task<Track?> GetAsync(Guid id);
}
