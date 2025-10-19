using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Abstractions;
public interface ILiveProjectionRepository
{
    public void RegisterManager(Type projectionServiceType, ILiveProjectionManager manager);
    public ILiveProjectionManager GetManager(Type projectionServiceType);
}
