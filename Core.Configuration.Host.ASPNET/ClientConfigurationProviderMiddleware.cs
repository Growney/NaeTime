using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Core.Configuration.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Configuration.Host.ASPNET
{
    public class ClientConfigurationProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _path;

        public ClientConfigurationProviderMiddleware(string path, RequestDelegate next)
        {
            _next = next;
            _path = path;
        }

        // TODO: Changed to use System.Text.Json when the merging is released in dotnet 6 
        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Method == HttpMethods.Get && context.Request.Path == _path)
            {
                var providers = context.RequestServices.GetService<IEnumerable<IClientConfigurationProvider>>();
                if (providers != null)
                {
                    StringBuilder jsonBuilder = new StringBuilder("{");
                    foreach (var provider in providers)
                    {
                        var configurationObj = provider.GetConfiguration();
                        var configurationJson = JObject.FromObject(configurationObj);

                        jsonBuilder.Append($"\"{provider.Name}\": {configurationJson}");


                    }
                    jsonBuilder.Append("}");
                    var jsonString = jsonBuilder.ToString();

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    await context.Response.WriteAsync(jsonString);
                    return;
                }

            }

            await _next(context);


        }
    }
}
