using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Core.Security.Core
{
    public interface ISecurityKeyProvider
    {
        Task<RsaSecurityKey> GetKeyAsync(Guid keyID);
    }
}
