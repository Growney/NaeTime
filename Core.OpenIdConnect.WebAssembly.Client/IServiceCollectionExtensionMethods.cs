using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Core.OpenIdConnect.Configuration;

namespace Core.OpenIdConnect.WebAssembly.Client
{
    public static class IServiceCollectionExtensionMethods
    {

        private static void CopyProviderOptions(OidcProviderOptions to, ProviderOptions from)
        {
            to.Authority = ReplaceIfNotNull(to.Authority, from.Authority);
            to.ClientId = ReplaceIfNotNull(to.ClientId, from.ClientId);
            if (from.DefaultScopes != null)
            {
                foreach (var defaultScope in from.DefaultScopes)
                {
                    to.DefaultScopes.Add(defaultScope);
                }
            }
            to.MetadataUrl = ReplaceIfNotNull(to.MetadataUrl, from.MetadataUrl);
            to.PostLogoutRedirectUri = ReplaceIfNotNull(to.PostLogoutRedirectUri, from.PostLogoutRedirectUri);
            to.RedirectUri = ReplaceIfNotNull(to.RedirectUri, from.RedirectUri);
            to.ResponseMode = ReplaceIfNotNull(to.ResponseMode, from.ResponseMode);
            to.ResponseType = ReplaceIfNotNull(to.ResponseType, from.ResponseType);
        }
        private static void CopyPathOptions(RemoteAuthenticationApplicationPathsOptions to, PathOptions from)
        {
            to.LogInCallbackPath = ReplaceIfNotNull(to.LogInCallbackPath, from.LogInCallbackPath);
            to.LogInFailedPath = ReplaceIfNotNull(to.LogInFailedPath, from.LogInFailedPath);
            to.LogInPath = ReplaceIfNotNull(to.LogInPath, from.LogInPath);

            to.LogOutCallbackPath = ReplaceIfNotNull(to.LogOutCallbackPath, from.LogOutCallbackPath);
            to.LogOutFailedPath = ReplaceIfNotNull(to.LogOutFailedPath, from.LogOutFailedPath);
            to.LogOutPath = ReplaceIfNotNull(to.LogOutPath, from.LogOutPath);
            to.LogOutSucceededPath = ReplaceIfNotNull(to.LogOutSucceededPath, from.LogOutSucceededPath);

            to.ProfilePath = ReplaceIfNotNull(to.ProfilePath, from.ProfilePath);
            to.RegisterPath = ReplaceIfNotNull(to.RegisterPath, from.RegisterPath);
            to.RemoteProfilePath = ReplaceIfNotNull(to.RemoteProfilePath, from.RemoteProfilePath);
            to.RemoteRegisterPath = ReplaceIfNotNull(to.RemoteRegisterPath, from.RemoteRegisterPath);
        }
        private static void CopyUserOptions(RemoteAuthenticationUserOptions to, UserOptions from)
        {
            to.AuthenticationType = ReplaceIfNotNull(to.AuthenticationType, from.AuthenticationType);
            to.NameClaim = ReplaceIfNotNull(to.NameClaim, from.NameClaim);
            to.RoleClaim = ReplaceIfNotNull(to.RoleClaim, from.RoleClaim);
            to.ScopeClaim = ReplaceIfNotNull(to.ScopeClaim, from.ScopeClaim);
        }

        private static string ReplaceIfNotNull(string currentValue, string newValue) => string.IsNullOrWhiteSpace(newValue) ? currentValue : newValue;

        public static IServiceCollection AddRemoteConfiguredOidcAuthentication(this IServiceCollection services)
        {
            services.AddOptions<OpenIdConnectOptions>().BindConfiguration("OpenIdConnect");
            services.AddOptions<RemoteAuthenticationOptions<OidcProviderOptions>>().PostConfigure<IOptions<OpenIdConnectOptions>>((configuration, options) =>
            {
                CopyProviderOptions(configuration.ProviderOptions, options.Value.ProviderOptions);
                CopyPathOptions(configuration.AuthenticationPaths, options.Value.AuthenticationPaths);
                CopyUserOptions(configuration.UserOptions, options.Value.UserOptions);
            });
            services.AddOidcAuthentication(x => { });

            return services;
        }
    }
}
