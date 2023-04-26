using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;
using Core.Serialization.Json;
using EventSourcingCore.CommandHandler.Abstractions;

namespace EventSourcingCore.CommandHandler.ASPNET.Tests.Implementations
{
    public static class TestHelper
    {
        public static async Task<HttpContext> CreateCommandContext(ICommand command, string identifier)
        {
            ServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddASPNetCommandRouting();

            ServiceProvider provider = serviceCollection.BuildServiceProvider();

            DefaultHttpContext context = new DefaultHttpContext
            {
                RequestServices = provider
            };
            context.Request.Method = "POST";

            JsonSerializer serializer = new JsonSerializer();

            byte[] commandBytes = serializer.Serialize(command);

            MemoryStream stream = new MemoryStream();
            await stream.WriteAsync(commandBytes);
            stream.Position = 0;

            context.Request.Headers.Add("Command-Identifier", identifier);
            context.Request.Body = stream;

            context.Response.Body = new MemoryStream();

            return context;

        }


        public static string GetMetadataString(CommandMetadata metadata)
        {
            JsonSerializer serializer = new JsonSerializer();
            byte[] dataBytes = serializer.Serialize(metadata);

            return Convert.ToBase64String(dataBytes);
        }
    }
}
