using NaeTime.Persistence.Models;

namespace NaeTime.Persistence.Abstractions;
public interface ISessionRepository
{
    public Task<IEnumerable<SessionDetails>> GetDetails();
    public Task<SessionDetails?> GetDetails(Guid id);
}
