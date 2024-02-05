using NaeTime.Timing.Models;

namespace NaeTime.Timing.Abstractions;
public interface ISessionFactory
{
    public ISession CreateSession(SessionType type, Guid id);
}
