using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Core.OpenIdConnect.Configuration
{
    public class ProviderOptions
    {
        public string Authority { get; set; }
        public string MetadataUrl { get; set; }
        public string ClientId { get; set; }
        public IList<string> DefaultScopes { get; }
        public string RedirectUri { get; set; }
        public string PostLogoutRedirectUri { get; set; }
        public string ResponseType { get; set; }
        public string ResponseMode { get; set; }
    }
}
