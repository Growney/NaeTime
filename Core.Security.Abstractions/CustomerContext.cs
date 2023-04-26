using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Abstractions
{
    public struct CustomerContext
    {
        public Guid CustomerID { get; }

        public CustomerContext(Guid customerID)
        {
            CustomerID = customerID;
        }
    }
}
