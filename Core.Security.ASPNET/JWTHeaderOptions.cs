using System;
using System.Collections.Generic;
using System.Text;
using Core.Security.Core;

namespace Core.Security.ASPNET
{
    public class JWTHeaderOptions
    {
        public string HeaderName { get; set; } = "Authorization";
    }
}
