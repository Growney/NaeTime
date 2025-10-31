using EventDbLite.Abstractions;
using System.Collections.Concurrent;

namespace EventDbLite.Projections;
public class LiveProjectionRepository : ILiveProjectionRepository
{
    private ConcurrentDictionary<Type, ILiveProjectionManager> _managers = new();
    public ILiveProjectionManager? GetManager(Type projectionServiceType)
    {
        if (!_managers.TryGetValue(projectionServiceType, out var manager))
        {
            return null;
        }
        return manager;
    }

    public void RegisterManager(Type projectionServiceType, ILiveProjectionManager manager)
    {
        if (!_managers.TryAdd(projectionServiceType, manager))
        {
            throw new InvalidOperationException($"A live projection manager is already registered for projection service type {projectionServiceType.FullName}");
        }
    }
}
