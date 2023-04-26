using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace Core.Security.Core
{
    public static class JWTHelper
    {
        public static SecurityTokenDescriptor CreateSecurityTokenDescriptor(SigningCredentials credentials, int lifespanInMinutes, string issuer, ClaimsCollection claims)
        {
            var now = DateTime.UtcNow;
            SecurityTokenDescriptor descriptor = new SecurityTokenDescriptor()
            {
                SigningCredentials = credentials,
                IssuedAt = now,
                Expires = now.AddMinutes(lifespanInMinutes),
                Issuer = issuer,
                Subject = new ClaimsIdentity(claims.GetClaims())
            };

            return descriptor;
        }
        public static string CreateEncodedJWT(SigningCredentials credentials, int lifespanInMinutes, string issuer, ClaimsCollection claims)
        {
            var descriptor = CreateSecurityTokenDescriptor(credentials, lifespanInMinutes, issuer, claims);
            return CreateEncodedJWT(descriptor);
        }
        public static string CreateEncodedJWT(SecurityTokenDescriptor descriptor)
        {
            var handler = new JwtSecurityTokenHandler();
            return handler.CreateEncodedJwt(descriptor);
        }
        public static bool TryGetClaims(string jwt, out IEnumerable<Claim> claims)
        {
            var handler = new JwtSecurityTokenHandler();
            claims = null;
            if (handler.CanReadToken(jwt))
            {
                try
                {
                    var priniciple = handler.ReadJwtToken(jwt);
                    claims = priniciple.Claims;
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }

        public static bool TryValidateToken(string jwt, TokenValidationParameters parameters, out IEnumerable<Claim> claims)
        {
            var handler = new JwtSecurityTokenHandler();
            claims = null;
            if (handler.CanReadToken(jwt))
            {
                try
                {
                    var priniciple = handler.ValidateToken(jwt, parameters, out var validToken);
                    claims = priniciple.Claims;
                    return true;
                }
                catch
                {

                }
            }
            return false;
        }
    }
}
