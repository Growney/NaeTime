using Microsoft.AspNetCore.Builder;
using System;
using EventSourcingCore.CommandHandler.Abstractions;
using EventSourcingCore.CommandHandler.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace EventSourcingCore.CommandHandler.ASPNET
{
    public static class IApplicationBuilderExtensionMethods
    {
        public static IApplicationBuilder UseASPNetCommandRouting(this IApplicationBuilder app)
        {
            if (app == null)
            {
                throw new ArgumentNullException(nameof(app));
            }

            IOptions<WebCommandHandlerOptions> options = app.ApplicationServices.GetRequiredService<IOptions<WebCommandHandlerOptions>>();

            if (string.IsNullOrWhiteSpace(options.Value.HandlePath))
            {
                throw new InvalidOperationException("Handle path is not configured");
            }

            app.Map($"/{options.Value.HandlePath}", x =>
            {
                IServiceProvider services = x.ApplicationServices;
                IHttpRequestCommandHandler handler = services.GetRequiredService<IHttpRequestCommandHandler>();
                x.Run(context =>
                {
                    return handler.Handle(context);
                });
            });

            return app;

        }


    }
}
