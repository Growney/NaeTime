using System;
using System.Collections.Generic;
using System.Text;

namespace EventSourcingCore.Projection.Core
{
    public interface IProjectionManagerBuilderProvider
    {
        IProjectionManagerBuilder GetProjectionManager(string key);
        IEnumerable<IProjectionManagerBuilder> GetAll();
        bool AddBuilder(IProjectionManagerBuilder builder);
    }
}
