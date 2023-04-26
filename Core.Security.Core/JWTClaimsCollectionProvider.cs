using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Core.Security.Core
{
    public class JWTClaimsCollectionProvider : IClaimsCollectionProvider
    {
        private readonly ISecurityKeyProvider _keyProvider;
        private readonly IOptions<JWTOptions> _options;

        private ClaimsCollection _claims;
        public JWTClaimsCollectionProvider(ISecurityKeyProvider keyProvider, IOptions<JWTOptions> options)
        {
            _keyProvider = keyProvider;
            _options = options;
        }

        public ClaimsCollection GetClaims()
        {
            return _claims;
        }


        public async Task SetClaimsAsync(string jwt)
        {

            if (JWTHelper.TryGetClaims(jwt, out var claims))
            {
                var collection = new ClaimsCollection(claims);

                if (collection.TryGetValue("KeyGuid", out string guidString))
                {
                    if (Guid.TryParse(guidString, out var keyGuid))
                    {
                        var key = await _keyProvider.GetKeyAsync(keyGuid);

                        var parameters = new TokenValidationParameters()
                        {
                            ValidIssuers = _options.Value.Issuers,
                            ValidateAudience = false,
                            IssuerSigningKey = key,
                        };

                        if (JWTHelper.TryValidateToken(jwt, parameters, out var _))
                        {
                            _claims = collection;
                        }
                        else
                        {
                            throw new UnauthorizedAccessException("Invalid_JWT");
                        }
                    }
                    else
                    {
                        throw new UnauthorizedAccessException("Invalid_KeyGuid");
                    }
                }
                else
                {
                    throw new UnauthorizedAccessException("No_KeyGuid");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("JWT_Read_Failed");
            }
        }
    }
}
