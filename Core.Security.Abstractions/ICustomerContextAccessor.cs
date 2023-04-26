using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Abstractions
{
    public interface ICustomerContextAccessor
    {
        CustomerContext Context { get; set; }
    }
}
