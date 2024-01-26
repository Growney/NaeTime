using NaeTime.Client.Razor.Lib.Models;

namespace NaeTime.Client.Razor.Lib.Abstractions;
public interface IFlyingSessionApiClient
{
    Task<IEnumerable<FlyingSession>> GetAllAsync();
    Task<FlyingSession?> GetAsync(Guid id);
    Task<FlyingSession?> CreateAsync(string description, DateTime start, DateTime expectedEnd, Guid trackId);
    Task<FlyingSession?> UpdateAsync(FlyingSession flyingSession);
}
