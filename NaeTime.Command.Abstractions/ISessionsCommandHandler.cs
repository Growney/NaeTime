using NaeTime.Events;
using System.Diagnostics.Tracing;

namespace NaeTime.Command.Abstractions;
public interface ISessionsCommandHandler
{
    public Task ActivateSession(Guid id, SessionType sessionType);
    public Task DeactivateSession(Guid id, SessionType sessionType);
}
