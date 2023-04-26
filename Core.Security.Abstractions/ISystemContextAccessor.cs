using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Abstractions
{
    public interface ISystemContextAccessor
    {
        public SystemContext Context { get; set; }
    }
}
