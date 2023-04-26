using EventSourcingCore.Tests.Implementations;
using Microsoft.IdentityModel.Tokens;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Core.Security.Core;
using Core.Serialization.Json;
using EventSourcingCore.CommandHandler.Abstractions;
using Xunit;

namespace EventSourcingCore.Tests.Implementations
{
    public static class TestHelper
    {
        public static string PublicKey { get; } = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEApEZj5qNDoM7+XA3ik1lI1lvzlAlmHd7e/3CS1SlVt9QOCulm0TgHf5ba10LCX84iSxK/qPGYaH4rFCtUeT+CPUXWxM4xZt4F0aQ1zIj+bH5vXWRsLb6DbXXcgBHDUtHbm0f+5AuPs+6o7FEHTDqAB0ygFhLfPjCD9wEmDTHDloa6LMXgcAPVfd7QGG/kIHOnr0RzsHM4JUV5wBRQBn7e7ebdUhI9sLBe8rTlRIvF0ZLniJ9jfwSW5gXmjq6ToHlDdchQcESqvQx+9+k4FEjF327vZETRMEehAnbwJgzTyl9RG3MEpUXzP1bGjO1ngtO56+fAqYywG51eS7J3pW7FdQIDAQAB";
        public static string PrivateKey { get; } = "MIIEpAIBAAKCAQEApEZj5qNDoM7+XA3ik1lI1lvzlAlmHd7e/3CS1SlVt9QOCulm0TgHf5ba10LCX84iSxK/qPGYaH4rFCtUeT+CPUXWxM4xZt4F0aQ1zIj+bH5vXWRsLb6DbXXcgBHDUtHbm0f+5AuPs+6o7FEHTDqAB0ygFhLfPjCD9wEmDTHDloa6LMXgcAPVfd7QGG/kIHOnr0RzsHM4JUV5wBRQBn7e7ebdUhI9sLBe8rTlRIvF0ZLniJ9jfwSW5gXmjq6ToHlDdchQcESqvQx+9+k4FEjF327vZETRMEehAnbwJgzTyl9RG3MEpUXzP1bGjO1ngtO56+fAqYywG51eS7J3pW7FdQIDAQABAoIBAA7Km8Ivny5ClRqa0XTtDCbo/qSst/omkDO14jK5VxOHT4BiBbT+84Xkgm3xf+j6eZIC7Sj8H/oAqkZVpHSKaGK2ACGWY1Jc2W9H9uQkzavdMo/ZM/hmeBUKEMGjV9AEP17a+7Ny6wyLh8BHh32wosiFeMCkEF/cJHXAL7nuaisKb5AJEtdhd0SC5SR6CPcoiwHID8hFmRN6rHqvkiEL7sMMcMhe6aeI0mxow/cAb33FB72K4OjqA7ja/z/MAnFoFqFh5lsTgHP1M5NsxXa0Ax7y54TKLS/38XzIf97lHj5sKZ6+06TLBhACBU0MEPBuMo4xw9m/jE3vx3wxWIm9MCECgYEA1ie6mUbgK1/udJ2fezb3xFeRbmVFvl32A4DCtRP/HGho/MS9oTjxAwwXv1SHgeqBOKLkkJCz8fpx9ckJsuaYnhVMh7yf4TZPCjGHAUz3OSiKkd2XZBwWU/sgK60rnBgf6slnGjsK1i+tldz8cxSINIj9RTzAKuMETPyArMEu4P0CgYEAxF+ZXGX92apGLtKzBUwoMjKlztWNHnSBDMTZDarI/25nSD6Pp1Zdnr5CjKJNIF0L+luo7kVX8sEIKFqbtra7HlmmREyo7dFJQTWZ7AKO8Yl4MUoKvieJFlXRgvKRbmDNFcNitQcSr/Pvlifr1hW+1RAg1qLWD+/tu16weyai+9kCgYBOmepdE6x3fxBQcQH2AWjTAaj5MeZ1RptjEcFlIN/Dl3bhP3yyTen3ylp0+Rt7TYz8Mp1dih7hs6BcK9uJdCWT8BFUyKCN0pcUELWSfqNGsWCaxRApyD1RuZxHK5oUAq7ESAO1lvIVRw6ZMLDftCuBzL3YycFmfEg50XuQ7J6+KQKBgQCuspmdwMJvpPKg7yMCIHlWBwbJFeRgZqbz9B9g8EG2M8LAW5+y77uD4KEVucHBe+WPCYIkzx3pwV5/f/QtPS0EWB75ffV+9IQl9giEFNaT/Icn2kXqWwOzEg+8Xg2RU56/sL2cbLlsPSg0vjkpkNjfdWSxbdXgAcPNv/Ri6sFuKQKBgQDMLJ9JonAGexL8+3H/ek4vUcOUmMBX0pCFjUrz0QSLk7CjaU6ivw198nOoZy3pYpcXgnyZGlRTbCchrhuxQ6w8wm/7KUYPuPW1NuXtsrNGrNlMBgJmYkDj2VFFCIkiNw+pz5chJ86LXWRmxjvCQo+1fXp8jxhf+YyXWD2SCsnU5Q==";
        public static Guid CustomerID { get; } = new Guid("746563b5-e5b7-4bd9-9238-57c329e96ba2");

        public static Guid EmployeeOneID { get; } = new Guid("7d53c194-b98d-4e02-b5f6-15d3f8a65ea9");
        public static Guid EmployeeTwoID { get; } = new Guid("826fb360-d5bb-4553-8ee9-2cdaeaf0105e");
        public static Guid EmployeeThreeD { get; } = new Guid("e5a4026d-0905-4538-b07e-f7cd0a6da24f");

        public static Guid UserOneID { get; } = new Guid("74bee47f-aac1-42d4-b8e4-c35c30f7ce69");
        public static Guid UserTwoID { get; } = new Guid("071125f5-e93b-4c31-8f06-92a673c5d3c7");
        public static Guid UserThreeID { get; } = new Guid("5959cacc-4ccc-47a5-b3d5-3cf2f4e0dd90");
        public static Guid UserFourID { get; } = new Guid("1ef6f056-0074-46de-81e4-a47bd43653d9");
        public static Guid UserFiveID { get; } = new Guid("9f6ea8b9-874e-499e-a081-f357dcd9bab9");

        public static Task<HttpResponseMessage> PostCommandAsync(this HttpClient client, Guid userID, ICommand command)
        {
            return client.PostCommandAsync(userID, command, command.GetType().Name);
        }
        public static Task<HttpResponseMessage> PostCommandAsync(this HttpClient client, Guid userID, ICommand command, string identifier)
        {
            ZonedClock zonedClock = new ZonedClock(SystemClock.Instance, DateTimeZoneProviders.Tzdb.GetSystemDefault(), CalendarSystem.Iso);
            ZonedDateTime userTime = zonedClock.GetCurrentZonedDateTime();
            var metadata = new CommandMetadata(identifier, userTime, userTime);
            var metadataString = GetMetadataString(metadata);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", GetJWT(userID, CustomerID));
            client.DefaultRequestHeaders.Add("Tensor-Command-Metadata", metadataString);

            var serializer = new JsonSerializer();
            var commandString = serializer.SerializeToString(command);
            return client.PostAsync("/Handle", new StringContent(commandString));
        }
        public static string GetMetadataString(CommandMetadata metadata)
        {
            JsonSerializer serializer = new JsonSerializer();
            byte[] dataBytes = serializer.Serialize(metadata);

            return Convert.ToBase64String(dataBytes);
        }

        public static Task<HttpResponseMessage> SendCommand(this TestWebApplicationFactory<FakeWebStartup> factory, Guid userID, ICommand command)
        {
            var client = factory.CreateClient();
            return client.PostCommandAsync(userID, command);
        }

        public static async Task AssertCommandSuccess(this TestWebApplicationFactory<FakeWebStartup> factory, Guid userID, ICommand command)
        {
            var response = await factory.SendCommand(userID, command);
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
        }

        public static async Task AssertCommandCode(this TestWebApplicationFactory<FakeWebStartup> factory, Guid userID, ICommand command, HttpStatusCode code, string message = null)
        {
            var response = await factory.SendCommand(userID, command);
            Assert.Equal(code, response.StatusCode);
            if (message != null)
            {
                var responseMessage = await response.Content.ReadAsStringAsync();
                Assert.Equal(message, responseMessage);
            }
        }

        public static string GetJWT(Guid userID, Guid customerID)
        {
            var key = RSAHelper.GetPEMEncodedRSAPrivateKey(PrivateKey);
            var signingCredentials = new SigningCredentials(key, "RS512");
            var claims = ClaimsCollection.CreateCollection(new KeyValuePair<string, object>("UserID", userID), new KeyValuePair<string, object>("CustomerID", customerID), new KeyValuePair<string, object>("KeyGuid", Guid.NewGuid()));
            var jwt = JWTHelper.CreateEncodedJWT(signingCredentials, 10, "Tensor", claims);

            return jwt;
        }
    }
}
