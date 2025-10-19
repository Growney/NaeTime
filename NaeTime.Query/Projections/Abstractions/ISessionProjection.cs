using NaeTime.Query.Abstractions.Models;

namespace NaeTime.Query.Projections.Abstractions;
public interface ISessionProjection
{
    Session? GetActiveSession();
    IEnumerable<Session> GetAllSessions();
    Session? GetSessionById(Guid sessionId);
}