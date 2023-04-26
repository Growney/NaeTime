using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Abstractions;

namespace Core.Security.Core
{
    public class CustomerContextAccessor : ICustomerContextAccessor
    {
        public CustomerContext Context { get; set; }
    }
}
