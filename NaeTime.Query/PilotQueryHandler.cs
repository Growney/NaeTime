using EventDbLite.Abstractions;
using NaeTime.Query.Abstractions;
using NaeTime.Query.Abstractions.Models;
using NaeTime.Query.Abstractions.Projections;

namespace NaeTime.Query;

public class PilotQueryHandler : IPilotQueryHandler
{
    private readonly IProjectionProvider _projectionProvider;

    public PilotQueryHandler(IProjectionProvider projectionProvider)
    {
        _projectionProvider = projectionProvider;
    }

    public Task<IEnumerable<Pilot>> GetAllPilots() =>
        _projectionProvider.ClonePullReadPushAsync<IEnumerable<Pilot>, IPilotProjection>(x => x.GetAllPilots());

    public Task<Pilot?> GetPilotById(Guid id) =>
        _projectionProvider.ClonePullReadPushAsync<Pilot?, IPilotProjection>(x => x.GetPilotById(id));

    public Task<byte[]?> GetPilotBindingPhrase(Guid id) =>
        _projectionProvider.ClonePullReadPushAsync<byte[]?, IPilotProjection>(x => x.GetPilotBindingPhrase(id));
}