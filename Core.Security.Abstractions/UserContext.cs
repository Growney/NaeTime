using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Abstractions
{
    public struct UserContext
    {
        public Guid UserID { get; }
        public UserContext(Guid userID)
        {
            UserID = userID;
        }
    }
}
