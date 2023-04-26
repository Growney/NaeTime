using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using EventSourcingCore.Readiness.Abstractions;
using EventSourcingCore.Readiness.Core;

namespace EventSourcingCore.Readiness.ASPNET
{
    public static class IApplicationBuilderExtensionMethods
    {
        public static IApplicationBuilder UseTensorReadinessCheck(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            var serviceProvider = app.ApplicationServices;

            IOptions<WebReadinessOptions> options = serviceProvider.GetRequiredService<IOptions<WebReadinessOptions>>();

            if (string.IsNullOrWhiteSpace(options.Value.ReadinessPath))
            {
                throw new InvalidOperationException("Readiness path is not configured");
            }

            app.Map($"/{options.Value.ReadinessPath}", x =>
            {
                IServiceProvider services = x.ApplicationServices;
                x.Run(async context =>
                {
                    bool isReady = true;
                    StringBuilder results = new StringBuilder();
                    results.Append("<div>");
                    foreach (var readyResult in services.GetReadinessResults())
                    {
                        var result = await readyResult;

                        results.Append($"<h3>{result.Header}</h3>");
                        isReady &= AppendBodies(0, results, result.Body);

                    }
                    results.Append("</div>");
                    context.Response.StatusCode = isReady ? (int)HttpStatusCode.OK : (int)HttpStatusCode.ServiceUnavailable;

                    string title = $"<h1>Service is {(!isReady ? "not " : "")}ready</h1>";
                    await context.Response.WriteAsync(title + results.ToString());

                });
            });

            return app;
        }

        private static bool AppendBodies(int depth, StringBuilder to, IEnumerable<ReadinessResultBody> bodies)
        {
            bool retVal = true;
            if (bodies != null)
            {
                foreach (var result in bodies)
                {
                    retVal &= result.Success;
                    if (result.Success)
                    {
                        to.Append("<p>");
                    }
                    else
                    {
                        to.Append("<p style=\"color: red;\">");
                    }
                    if (depth > 0)
                    {
                        to.Append(RepeatPadLeft("&nbsp;&nbsp;&nbsp;&nbsp;", depth) + result.Status);
                    }
                    else
                    {
                        to.Append(result.Status);
                    }
                    retVal &= AppendBodies(depth + 1, to, result.Body);

                    to.Append("</p>");
                }
            }
            return retVal;
        }

        private static string RepeatPadLeft(string s, int n)
        {
            return "".PadLeft(n, 'X').Replace("X", s);
        }
    }
}
