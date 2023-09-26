
using EventStore.Helpers;
using NaeTime.Server.Abstractions.Events;

namespace NaeTime.Server.EventStore.Projections;
public class PassProjection : IProjection
{
    public void Configure(IProjectionHandler handler)
    {
        handler.On<NodeConfigured>(x =>
        {
            return Task.CompletedTask;
        });
    }
}
