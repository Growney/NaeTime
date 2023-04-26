using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Projection.Abstractions
{
    public enum ProjectionScope
    {
        Singleton,
        Scoped
    }
}
