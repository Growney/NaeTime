using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Xunit;

namespace Core.Security.Core.Tests
{
    public class JWTTests
    {

        [Fact]
        public void For_ValidTokenValidationParameters_When_TokenIsEmpty_Expect_SuccesfullValidation()
        {
            string base64Exponent = "AQAB";
            string base64Modulus = "nzyis1ZjfNB0bBgKFMSvvkTtwlvBsaJq7S5wA+kzeVOVpVWwkWdVha4s38XM/pa/yr47av7+z3VTmvDRyAHcaT92whREFpLv9cj5lTeJSibyr/Mrm/YtjCZVWgaOYIhwrXwKLqPr/11inWsAkfIytvHWTxZYEcXLgAXFuUuaS3uF9gEiNQwzGTU1v0FqkqTBr4B8nW3HCN47XUu0t8Y0e+lf4s4OxQawWD79J9/5d3Ry0vbV3Am1FtGJiJvOwRsIfVChDpYStTcHTCMqtvWbV6L11BWkpzGXSW4Hv43qa+GSYOD2QU68Mb59oSk2OB+BtOLpJofmbGEGgvmwyCI9Mw==";
            string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.e30.Ps-lkT0lKrfdrkHrN-efCNfmGAV-nLmO24xl7VBJMZZxRJes6P2fH9B4wQHRMgmjwvijpmYN4qBuFFtibtEYN0h3_KfO9IXi3FspoFfDCl1C3oVRE_OAsqW6k148TGTTZ28ozlzs2ngwLFJpt9TYmkkr_MOsIPpX6jT00iU5758CPo3Lj714JD8FFKch42Eokcdfbt8b_Rv7TYUaIFJsnrJu77Cei5Em1EFPjD91o58eQsHxMBRiQLODadOf5xrFC4LkOqkdat_l1dSYCGPUEg3PufHTqwDilIZVBujUUZzGE-EXqxumsrtVnU0oHz23tLgpZOfQZqqx2UvOKP63Yg";

            var parameters = new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new RsaSecurityKey(new RSAParameters()
                {
                    Exponent = Convert.FromBase64String(base64Exponent),
                    Modulus = Convert.FromBase64String(base64Modulus)
                })
            };

            Assert.True(JWTHelper.TryValidateToken(token, parameters, out _));
        }
        [Fact]
        public void For_ValidTokenValidationParameters_When_TokenIsEmpty_Expect_EmptyClaims()
        {
            string base64Exponent = "AQAB";
            string base64Modulus = "nzyis1ZjfNB0bBgKFMSvvkTtwlvBsaJq7S5wA+kzeVOVpVWwkWdVha4s38XM/pa/yr47av7+z3VTmvDRyAHcaT92whREFpLv9cj5lTeJSibyr/Mrm/YtjCZVWgaOYIhwrXwKLqPr/11inWsAkfIytvHWTxZYEcXLgAXFuUuaS3uF9gEiNQwzGTU1v0FqkqTBr4B8nW3HCN47XUu0t8Y0e+lf4s4OxQawWD79J9/5d3Ry0vbV3Am1FtGJiJvOwRsIfVChDpYStTcHTCMqtvWbV6L11BWkpzGXSW4Hv43qa+GSYOD2QU68Mb59oSk2OB+BtOLpJofmbGEGgvmwyCI9Mw==";
            string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.e30.Ps-lkT0lKrfdrkHrN-efCNfmGAV-nLmO24xl7VBJMZZxRJes6P2fH9B4wQHRMgmjwvijpmYN4qBuFFtibtEYN0h3_KfO9IXi3FspoFfDCl1C3oVRE_OAsqW6k148TGTTZ28ozlzs2ngwLFJpt9TYmkkr_MOsIPpX6jT00iU5758CPo3Lj714JD8FFKch42Eokcdfbt8b_Rv7TYUaIFJsnrJu77Cei5Em1EFPjD91o58eQsHxMBRiQLODadOf5xrFC4LkOqkdat_l1dSYCGPUEg3PufHTqwDilIZVBujUUZzGE-EXqxumsrtVnU0oHz23tLgpZOfQZqqx2UvOKP63Yg";

            var parameters = new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new RsaSecurityKey(new RSAParameters()
                {
                    Exponent = Convert.FromBase64String(base64Exponent),
                    Modulus = Convert.FromBase64String(base64Modulus)
                })
            };

            JWTHelper.TryValidateToken(token, parameters, out var claims);

            Assert.Empty(claims);
        }
        [Fact]
        public void For_ValidTokenValidationParameters_When_TokenContainsTwoFields_Expect_SuccesfullValidation()
        {
            string base64Exponent = "AQAB";
            string base64Modulus = "nzyis1ZjfNB0bBgKFMSvvkTtwlvBsaJq7S5wA+kzeVOVpVWwkWdVha4s38XM/pa/yr47av7+z3VTmvDRyAHcaT92whREFpLv9cj5lTeJSibyr/Mrm/YtjCZVWgaOYIhwrXwKLqPr/11inWsAkfIytvHWTxZYEcXLgAXFuUuaS3uF9gEiNQwzGTU1v0FqkqTBr4B8nW3HCN47XUu0t8Y0e+lf4s4OxQawWD79J9/5d3Ry0vbV3Am1FtGJiJvOwRsIfVChDpYStTcHTCMqtvWbV6L11BWkpzGXSW4Hv43qa+GSYOD2QU68Mb59oSk2OB+BtOLpJofmbGEGgvmwyCI9Mw==";
            string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIiLCJDdXN0b21lcklEIjoiIn0.X8EHqH8BwVHZV-Y36bcmKsJttfMZvwdnqZWaSHljDVi6CznbIT_PoaAg8hZ-uek9GlAUI5U1XNvxpreXDwJRPkhDgwxMQ7sQ8XtLu_h2wvePd6Op7pWvsoXJrNdE7PGHYRrctGbg59c7ZD2Nhxx2WpcNVmuMdJG8XGJHoOam8-80eG-Sd6ykI_SdSOyz7jxBFKpUcjy1_cYMyTrK7Fu4PgdOBI7iaA7DFiYHgyjaam8RFz-r3u1WhXBPoiDpxpbdXUuOriF3PdXPhkNH-rSpqiLlZ5OmC8-uC9_SZeuJRiWsif2H8QySPWE8C0cQZfB40BfJBudswG9TQuX2WAAzyA";

            var parameters = new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new RsaSecurityKey(new RSAParameters()
                {
                    Exponent = Convert.FromBase64String(base64Exponent),
                    Modulus = Convert.FromBase64String(base64Modulus)
                })
            };

            Assert.True(JWTHelper.TryValidateToken(token, parameters, out _));
        }
        [Fact]
        public void For_ValidTokenValidationParameters_When_TokenContainsTwoFields_Expect_EmptyClaims()
        {
            string base64Exponent = "AQAB";
            string base64Modulus = "nzyis1ZjfNB0bBgKFMSvvkTtwlvBsaJq7S5wA+kzeVOVpVWwkWdVha4s38XM/pa/yr47av7+z3VTmvDRyAHcaT92whREFpLv9cj5lTeJSibyr/Mrm/YtjCZVWgaOYIhwrXwKLqPr/11inWsAkfIytvHWTxZYEcXLgAXFuUuaS3uF9gEiNQwzGTU1v0FqkqTBr4B8nW3HCN47XUu0t8Y0e+lf4s4OxQawWD79J9/5d3Ry0vbV3Am1FtGJiJvOwRsIfVChDpYStTcHTCMqtvWbV6L11BWkpzGXSW4Hv43qa+GSYOD2QU68Mb59oSk2OB+BtOLpJofmbGEGgvmwyCI9Mw==";
            string token = "eyJhbGciOiJSUzI1NiIsInR5cCI6IkpXVCJ9.eyJVc2VySUQiOiIiLCJDdXN0b21lcklEIjoiIn0.X8EHqH8BwVHZV-Y36bcmKsJttfMZvwdnqZWaSHljDVi6CznbIT_PoaAg8hZ-uek9GlAUI5U1XNvxpreXDwJRPkhDgwxMQ7sQ8XtLu_h2wvePd6Op7pWvsoXJrNdE7PGHYRrctGbg59c7ZD2Nhxx2WpcNVmuMdJG8XGJHoOam8-80eG-Sd6ykI_SdSOyz7jxBFKpUcjy1_cYMyTrK7Fu4PgdOBI7iaA7DFiYHgyjaam8RFz-r3u1WhXBPoiDpxpbdXUuOriF3PdXPhkNH-rSpqiLlZ5OmC8-uC9_SZeuJRiWsif2H8QySPWE8C0cQZfB40BfJBudswG9TQuX2WAAzyA";

            var parameters = new TokenValidationParameters()
            {
                ValidateLifetime = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                IssuerSigningKey = new RsaSecurityKey(new RSAParameters()
                {
                    Exponent = Convert.FromBase64String(base64Exponent),
                    Modulus = Convert.FromBase64String(base64Modulus)
                })
            };

            JWTHelper.TryValidateToken(token, parameters, out var claims);

            Assert.Equal(2, claims.Count());
        }
        [Fact]
        public void For_CreateTokenWithCustomClaims_When_TokenContainsUserAndCustomerID_Expect_ClaimsOnObject()
        {
            string publicKeyString = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApEZj5qNDoM7+XA3ik1lI1lvzlAlmHd7e/3CS1SlVt9QOCulm0TgHf5ba10LCX84iSxK/qPGYaH4rFCtUeT+CPUXWxM4xZt4F0aQ1zIj+bH5vXWRsLb6DbXXcgBHDUtHbm0f+5AuPs+6o7FEHTDqAB0ygFhLfPjCD9wEmDTHDloa6LMXgcAPVfd7QGG/kIHOnr0RzsHM4JUV5wBRQBn7e7ebdUhI9sLBe8rTlRIvF0ZLniJ9jfwSW5gXmjq6ToHlDdchQcESqvQx+9+k4FEjF327vZETRMEehAnbwJgzTyl9RG3MEpUXzP1bGjO1ngtO56+fAqYywG51eS7J3pW7FdQIDAQAB";
            string privateKeyString = "MIIEpAIBAAKCAQEApEZj5qNDoM7+XA3ik1lI1lvzlAlmHd7e/3CS1SlVt9QOCulm0TgHf5ba10LCX84iSxK/qPGYaH4rFCtUeT+CPUXWxM4xZt4F0aQ1zIj+bH5vXWRsLb6DbXXcgBHDUtHbm0f+5AuPs+6o7FEHTDqAB0ygFhLfPjCD9wEmDTHDloa6LMXgcAPVfd7QGG/kIHOnr0RzsHM4JUV5wBRQBn7e7ebdUhI9sLBe8rTlRIvF0ZLniJ9jfwSW5gXmjq6ToHlDdchQcESqvQx+9+k4FEjF327vZETRMEehAnbwJgzTyl9RG3MEpUXzP1bGjO1ngtO56+fAqYywG51eS7J3pW7FdQIDAQABAoIBAA7Km8Ivny5ClRqa0XTtDCbo/qSst/omkDO14jK5VxOHT4BiBbT+84Xkgm3xf+j6eZIC7Sj8H/oAqkZVpHSKaGK2ACGWY1Jc2W9H9uQkzavdMo/ZM/hmeBUKEMGjV9AEP17a+7Ny6wyLh8BHh32wosiFeMCkEF/cJHXAL7nuaisKb5AJEtdhd0SC5SR6CPcoiwHID8hFmRN6rHqvkiEL7sMMcMhe6aeI0mxow/cAb33FB72K4OjqA7ja/z/MAnFoFqFh5lsTgHP1M5NsxXa0Ax7y54TKLS/38XzIf97lHj5sKZ6+06TLBhACBU0MEPBuMo4xw9m/jE3vx3wxWIm9MCECgYEA1ie6mUbgK1/udJ2fezb3xFeRbmVFvl32A4DCtRP/HGho/MS9oTjxAwwXv1SHgeqBOKLkkJCz8fpx9ckJsuaYnhVMh7yf4TZPCjGHAUz3OSiKkd2XZBwWU/sgK60rnBgf6slnGjsK1i+tldz8cxSINIj9RTzAKuMETPyArMEu4P0CgYEAxF+ZXGX92apGLtKzBUwoMjKlztWNHnSBDMTZDarI/25nSD6Pp1Zdnr5CjKJNIF0L+luo7kVX8sEIKFqbtra7HlmmREyo7dFJQTWZ7AKO8Yl4MUoKvieJFlXRgvKRbmDNFcNitQcSr/Pvlifr1hW+1RAg1qLWD+/tu16weyai+9kCgYBOmepdE6x3fxBQcQH2AWjTAaj5MeZ1RptjEcFlIN/Dl3bhP3yyTen3ylp0+Rt7TYz8Mp1dih7hs6BcK9uJdCWT8BFUyKCN0pcUELWSfqNGsWCaxRApyD1RuZxHK5oUAq7ESAO1lvIVRw6ZMLDftCuBzL3YycFmfEg50XuQ7J6+KQKBgQCuspmdwMJvpPKg7yMCIHlWBwbJFeRgZqbz9B9g8EG2M8LAW5+y77uD4KEVucHBe+WPCYIkzx3pwV5/f/QtPS0EWB75ffV+9IQl9giEFNaT/Icn2kXqWwOzEg+8Xg2RU56/sL2cbLlsPSg0vjkpkNjfdWSxbdXgAcPNv/Ri6sFuKQKBgQDMLJ9JonAGexL8+3H/ek4vUcOUmMBX0pCFjUrz0QSLk7CjaU6ivw198nOoZy3pYpcXgnyZGlRTbCchrhuxQ6w8wm/7KUYPuPW1NuXtsrNGrNlMBgJmYkDj2VFFCIkiNw+pz5chJ86LXWRmxjvCQo+1fXp8jxhf+YyXWD2SCsnU5Q==";
            var privateKey = RSAHelper.GetPEMEncodedRSAPrivateKey(privateKeyString);
            var signingCredentials = new SigningCredentials(privateKey, "RS512");
            var claims = ClaimsCollection.CreateCollection(new KeyValuePair<string, object>("UserID", Guid.NewGuid()), new KeyValuePair<string, object>("CustomerID", Guid.NewGuid()));
            var jwt = JWTHelper.CreateEncodedJWT(signingCredentials, 10, "Tensor", claims);

            var publicKey = RSAHelper.GetPEMEncodedSubjectPublicKey(publicKeyString);

            var parameters = new TokenValidationParameters()
            {
                ValidIssuer = "Tensor",
                ValidateAudience = false,
                IssuerSigningKey = privateKey,
            };

            JWTHelper.TryValidateToken(jwt, parameters, out var validatedClaims);

            var validatedCollection = new ClaimsCollection(validatedClaims);

            Assert.True(validatedCollection.ContainsKey("UserID"));
            Assert.True(validatedCollection.ContainsKey("CustomerID"));
        }

        [Fact]
        public void For_CreateTokenWithCustomClaims_When_NoValidationTokenContainsUserAndCustomerID_Expect_ClaimsOnObject()
        {
            string privateKeyString = "MIIEpAIBAAKCAQEApEZj5qNDoM7+XA3ik1lI1lvzlAlmHd7e/3CS1SlVt9QOCulm0TgHf5ba10LCX84iSxK/qPGYaH4rFCtUeT+CPUXWxM4xZt4F0aQ1zIj+bH5vXWRsLb6DbXXcgBHDUtHbm0f+5AuPs+6o7FEHTDqAB0ygFhLfPjCD9wEmDTHDloa6LMXgcAPVfd7QGG/kIHOnr0RzsHM4JUV5wBRQBn7e7ebdUhI9sLBe8rTlRIvF0ZLniJ9jfwSW5gXmjq6ToHlDdchQcESqvQx+9+k4FEjF327vZETRMEehAnbwJgzTyl9RG3MEpUXzP1bGjO1ngtO56+fAqYywG51eS7J3pW7FdQIDAQABAoIBAA7Km8Ivny5ClRqa0XTtDCbo/qSst/omkDO14jK5VxOHT4BiBbT+84Xkgm3xf+j6eZIC7Sj8H/oAqkZVpHSKaGK2ACGWY1Jc2W9H9uQkzavdMo/ZM/hmeBUKEMGjV9AEP17a+7Ny6wyLh8BHh32wosiFeMCkEF/cJHXAL7nuaisKb5AJEtdhd0SC5SR6CPcoiwHID8hFmRN6rHqvkiEL7sMMcMhe6aeI0mxow/cAb33FB72K4OjqA7ja/z/MAnFoFqFh5lsTgHP1M5NsxXa0Ax7y54TKLS/38XzIf97lHj5sKZ6+06TLBhACBU0MEPBuMo4xw9m/jE3vx3wxWIm9MCECgYEA1ie6mUbgK1/udJ2fezb3xFeRbmVFvl32A4DCtRP/HGho/MS9oTjxAwwXv1SHgeqBOKLkkJCz8fpx9ckJsuaYnhVMh7yf4TZPCjGHAUz3OSiKkd2XZBwWU/sgK60rnBgf6slnGjsK1i+tldz8cxSINIj9RTzAKuMETPyArMEu4P0CgYEAxF+ZXGX92apGLtKzBUwoMjKlztWNHnSBDMTZDarI/25nSD6Pp1Zdnr5CjKJNIF0L+luo7kVX8sEIKFqbtra7HlmmREyo7dFJQTWZ7AKO8Yl4MUoKvieJFlXRgvKRbmDNFcNitQcSr/Pvlifr1hW+1RAg1qLWD+/tu16weyai+9kCgYBOmepdE6x3fxBQcQH2AWjTAaj5MeZ1RptjEcFlIN/Dl3bhP3yyTen3ylp0+Rt7TYz8Mp1dih7hs6BcK9uJdCWT8BFUyKCN0pcUELWSfqNGsWCaxRApyD1RuZxHK5oUAq7ESAO1lvIVRw6ZMLDftCuBzL3YycFmfEg50XuQ7J6+KQKBgQCuspmdwMJvpPKg7yMCIHlWBwbJFeRgZqbz9B9g8EG2M8LAW5+y77uD4KEVucHBe+WPCYIkzx3pwV5/f/QtPS0EWB75ffV+9IQl9giEFNaT/Icn2kXqWwOzEg+8Xg2RU56/sL2cbLlsPSg0vjkpkNjfdWSxbdXgAcPNv/Ri6sFuKQKBgQDMLJ9JonAGexL8+3H/ek4vUcOUmMBX0pCFjUrz0QSLk7CjaU6ivw198nOoZy3pYpcXgnyZGlRTbCchrhuxQ6w8wm/7KUYPuPW1NuXtsrNGrNlMBgJmYkDj2VFFCIkiNw+pz5chJ86LXWRmxjvCQo+1fXp8jxhf+YyXWD2SCsnU5Q==";
            var privateKey = RSAHelper.GetPEMEncodedRSAPrivateKey(privateKeyString);
            var signingCredentials = new SigningCredentials(privateKey, "RS512");
            var claims = ClaimsCollection.CreateCollection(new KeyValuePair<string, object>("UserID", Guid.NewGuid()), new KeyValuePair<string, object>("CustomerID", Guid.NewGuid()));
            var jwt = JWTHelper.CreateEncodedJWT(signingCredentials, 10, "Tensor", claims);

            JWTHelper.TryGetClaims(jwt, out var validatedClaims);

            var validatedCollection = new ClaimsCollection(validatedClaims);

            Assert.True(validatedCollection.ContainsKey("UserID"));
            Assert.True(validatedCollection.ContainsKey("CustomerID"));
        }
    }
}
