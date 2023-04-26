using System;

namespace Core.Security.Abstractions
{

    public interface IUserContextAccessor
    {
        public UserContext Context { get; set; }
    }
}
