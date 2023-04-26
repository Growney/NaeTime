using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Abstractions;

namespace Core.Security.Core
{
    public class UserContextAccessor : IUserContextAccessor
    {
        public UserContext Context { get; set; }
    }
}
