using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Core.Security.Core;
using System.Net;
using Core.Security.Abstractions;
using Microsoft.Extensions.Logging;

namespace Core.Security.ASPNET
{
    public static class IApplicationBuilderExtensionMethods
    {
        public static IApplicationBuilder UseJWTClaimsProvider(this IApplicationBuilder app, bool requireToken = false)
        {
            app.Use(async (context, next) =>
            {
                var headerOptions = context.RequestServices.GetService<IOptions<JWTHeaderOptions>>();
                var tokenOptions = context.RequestServices.GetService<IOptions<JWTOptions>>();

                bool authenticatedFailed = false;
                string failureMessage = null;

                if (context.Request.Headers.TryGetValue(headerOptions.Value.HeaderName, out var values))
                {
                    if (values.Count > 0)
                    {
                        var provider = context.RequestServices.GetService<IClaimsCollectionProvider>();

                        try
                        {
                            string credentials;
                            string type;
                            var tokenSplit = values[0].Split(' ');
                            if (tokenSplit.Length == 2)
                            {
                                credentials = tokenSplit[1];
                                type = tokenSplit[0];
                            }
                            else
                            {
                                credentials = tokenSplit[tokenSplit.Length - 1];
                            }


                            await provider.SetClaimsAsync(credentials);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            authenticatedFailed = true;
                            failureMessage = ex.Message;
                        }
                    }
                }
                else if (requireToken)
                {
                    authenticatedFailed = true;
                    failureMessage = "No_Authentication_Token";
                }

                if (authenticatedFailed)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    await context.Response.WriteAsync(failureMessage);
                }
                else
                {
                    await next();
                }
            });

            return app;
        }
        public static IApplicationBuilder UseSystemAuthentication(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var accessor = context.RequestServices.GetRequiredService<IClaimsCollectionProvider>();
                var claims = accessor.GetClaims();
                if (claims != null)
                {
                    if (claims.TryGetValue("TokenID", out string userValue))
                    {
                        if (Guid.TryParse(userValue, out Guid tokenID))
                        {
                            var userAccessor = context.RequestServices.GetRequiredService<ISystemContextAccessor>();
                            userAccessor.Context = new SystemContext(tokenID);

                            using (var scope = context.RequestServices.GetRequiredService<ILogger<IApplicationBuilder>>().BeginScope("TokenID: {tokenId}", tokenID))
                            {
                                await next();
                            }
                            return;
                        }
                    }
                }

                await next();
            });
            return app;
        }
        public static IApplicationBuilder UseUserAuthentication(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var accessor = context.RequestServices.GetRequiredService<IClaimsCollectionProvider>();
                var claims = accessor.GetClaims();
                if (claims != null)
                {
                    if (claims.TryGetValue("UserID", out string userValue))
                    {
                        if (Guid.TryParse(userValue, out Guid userID))
                        {
                            var userAccessor = context.RequestServices.GetRequiredService<IUserContextAccessor>();
                            userAccessor.Context = new UserContext(userID);

                            using (var scope = context.RequestServices.GetRequiredService<ILogger<IApplicationBuilder>>().BeginScope("UserId: {userID}", userID))
                            {
                                await next();
                            }
                            return;
                        }
                    }
                }

                await next();
            });

            return app;
        }
        public static IApplicationBuilder UseCustomerAuthentication(this IApplicationBuilder app)
        {
            app.Use(async (context, next) =>
            {
                var accessor = context.RequestServices.GetRequiredService<IClaimsCollectionProvider>();
                var claims = accessor.GetClaims();
                if (claims != null)
                {
                    if (claims.TryGetValue("CustomerID", out string userValue))
                    {
                        if (Guid.TryParse(userValue, out Guid customerID))
                        {
                            var customerAccessor = context.RequestServices.GetRequiredService<ICustomerContextAccessor>();
                            customerAccessor.Context = new CustomerContext(customerID);
                            using (var scope = context.RequestServices.GetRequiredService<ILogger<IApplicationBuilder>>().BeginScope("CustomerId: {customerID}", customerID))
                            {
                                await next();
                            }
                            return;
                        }
                    }
                }

                await next();
            });

            return app;
        }
        public static IApplicationBuilder UseJWTAuthentication(this IApplicationBuilder app, bool requireToken = false)
        {
            app.UseJWTClaimsProvider(requireToken);
            app.UseUserAuthentication();
            app.UseCustomerAuthentication();
            app.UseSystemAuthentication();

            return app;
        }
    }
}
