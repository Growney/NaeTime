using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Abstractions
{
    public class SystemContext
    {
        public SystemContext(Guid tokenID)
        {
            TokenID = tokenID;
        }

        public Guid TokenID { get; }
    }
}
