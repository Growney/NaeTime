using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Reflection.Abstractions;
using EventSourcingCore.Event.Abstractions;
using EventSourcingCore.Event.Abstractions.Metadata;
using EventSourcingCore.Projection.Abstractions;

namespace EventSourcingCore.Projection.Core
{
    public interface IProjectionManager
    {
        Task<bool> Start();
        void Register(IProjectionEventHandler handler);
    }
}
