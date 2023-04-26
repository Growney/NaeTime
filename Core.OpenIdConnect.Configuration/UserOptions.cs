using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.OpenIdConnect.Configuration
{
    public class UserOptions
    {
        public string NameClaim { get; set; }
        public string RoleClaim { get; set; }
        public string ScopeClaim { get; set; }
        public string AuthenticationType { get; set; }
    }
}
