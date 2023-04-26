using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Core.Security.Core;

namespace EventSourcingCore.Tests.Implementations
{
    public class FakeStaticKeyProvider : ISecurityKeyProvider
    {
        public Task<RsaSecurityKey> GetKeyAsync(Guid guid)
        {
            return Task.FromResult(RSAHelper.GetPEMEncodedSubjectPublicKey(TestHelper.PublicKey));
        }
    }
}
