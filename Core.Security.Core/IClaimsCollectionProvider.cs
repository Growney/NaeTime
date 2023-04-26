using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Core
{
    public interface IClaimsCollectionProvider
    {
        Task SetClaimsAsync(string jwt);
        ClaimsCollection GetClaims();
    }
}
