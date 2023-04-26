using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Core.Security.Core
{
    public static class RSAHelper
    {
        public static RsaSecurityKey GetPEMEncodedSubjectPublicKey(string publicKey)
        {
            var bytes = Convert.FromBase64String(publicKey);
            using (var rsa = RSA.Create())
            {
                rsa.ImportSubjectPublicKeyInfo(bytes, out _);
                var parameters = rsa.ExportParameters(false);
                return new RsaSecurityKey(parameters);
            }
        }

        public static RsaSecurityKey GetPEMEncodedRSAPublicKey(string publicKey)
        {
            var bytes = Convert.FromBase64String(publicKey);
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPublicKey(bytes, out _);
                var parameters = rsa.ExportParameters(false);
                return new RsaSecurityKey(parameters);
            }
        }

        public static RsaSecurityKey GetPEMEncodedRSAPrivateKey(string privateKey)
        {
            var bytes = Convert.FromBase64String(privateKey);
            using (var rsa = RSA.Create())
            {
                rsa.ImportRSAPrivateKey(bytes, out _);
                var parameters = rsa.ExportParameters(true);
                return new RsaSecurityKey(parameters);
            }
        }

    }
}
