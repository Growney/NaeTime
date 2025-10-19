using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EventDbLite.Abstractions;
public interface ILiveProjectionManager
{
    public Task WaitForVersion(long globalPosition, CancellationToken cancellationToken);
}
