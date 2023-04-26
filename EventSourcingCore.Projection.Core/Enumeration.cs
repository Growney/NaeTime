using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Projection.Core
{
    public enum ProjectionManagerStatus
    {
        Empty,
        WaitingToStart,
        FailedToStart,
        Running,
        CatchingUp
    }
}
