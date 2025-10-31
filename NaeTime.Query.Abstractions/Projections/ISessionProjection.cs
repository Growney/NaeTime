using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Abstractions.Projections;
public interface ISessionProjection
{
    Session? GetActiveSession();
    IEnumerable<Session> GetAllSessions();
    Session? GetSessionById(Guid sessionId);
}