using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Security.Core
{
    public class JWTOptions
    {
        public IEnumerable<string> Issuers { get; set; } = new List<string>() { "Tensor" };

    }
}
