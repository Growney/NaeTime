using EventDbLite.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Projections;
public class LiveProjectionRepository : ILiveProjectionRepository
{
    private ConcurrentDictionary<Type,ILiveProjectionManager> _managers = new();
    public ILiveProjectionManager GetManager(Type projectionServiceType)
    {
        if (_managers.TryGetValue(projectionServiceType, out var manager))
        {
            return manager;
        }
        throw new InvalidOperationException($"No live projection manager registered for projection service type {projectionServiceType.FullName}");
    }

    public void RegisterManager(Type projectionServiceType, ILiveProjectionManager manager)
    {
        if (!_managers.TryAdd(projectionServiceType, manager))
        {
            throw new InvalidOperationException($"A live projection manager is already registered for projection service type {projectionServiceType.FullName}");
        }
    }
}
